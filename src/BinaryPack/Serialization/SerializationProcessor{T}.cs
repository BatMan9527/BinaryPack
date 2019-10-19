﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Delegates;
using BinaryPack.Extensions;
using BinaryPack.Extensions.System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Extensions;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers
    /// </summary>
    /// <typeparam name="T">The type of items to handle during serialization and deserialization</typeparam>
    internal static class SerializationProcessor<T> where T : new()
    {
        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the serializer being built for type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinarySerializer<T>> _Serializer = DynamicMethod<BinarySerializer<T>>.New();

        /// <summary>
        /// Gets the <see cref="BinarySerializer{T}"/> instance for the current type <typeparamref name="T"/>
        /// </summary>
        public static BinarySerializer<T> Serializer { get; } = BuildSerializer();

        /// <summary>
        /// The <see cref="DynamicMethod{T}"/> instance holding the deserializer being built for type <typeparamref name="T"/>
        /// </summary>
        public static readonly DynamicMethod<BinaryDeserializer<T>> _Deserializer = DynamicMethod<BinaryDeserializer<T>>.New();

        /// <summary>
        /// Gets the <see cref="BinaryDeserializer{T}"/> instance for the current type <typeparamref name="T"/>
        /// </summary>
        public static BinaryDeserializer<T> Deserializer { get; } = BuildDeserializer();

        /// <summary>
        /// Builds a new <see cref="BinarySerializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        [Pure]
        private static BinarySerializer<T> BuildSerializer() => _Serializer.Build(il =>
        {
            il.DeclareLocalsFromType<Locals.Write>();

            // Null check if needed
            if (!typeof(T).IsValueType)
            {
                il.EmitSerializeIsNullFlag();
                il.EmitReturnIfNull();
            }

            // Properties serialization
            foreach (PropertyInfo property in
                from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead && prop.CanWrite
                select prop)
            {
                if (property.PropertyType.IsUnmanaged()) il.EmitSerializeUnmanagedProperty(property);
                else if (property.PropertyType == typeof(string)) il.EmitSerializeStringProperty(property);
                else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsUnmanaged()) il.EmitSerializeUnmanagedArrayProperty(property);
                else if (property.PropertyType.IsArray && !property.PropertyType.GetElementType().IsValueType)
                {
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitLoadArgument(Arguments.Write.Stream);
                    il.EmitCall(OpCodes.Call, KnownMembers.ArrayProcessor.SerializerInfo(property.PropertyType.GetElementType()), null);
                }
                else if (!property.PropertyType.IsValueType)
                {
                    il.EmitLoadArgument(Arguments.Write.T);
                    il.EmitReadMember(property);
                    il.EmitLoadArgument(Arguments.Write.Stream);
                    il.EmitCall(OpCodes.Call, KnownMembers.SerializationProcessor.SerializerInfo(property.PropertyType), null);
                }
                else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
            }

            il.Emit(OpCodes.Ret);
        });

        /// <summary>
        /// Builds a new <see cref="BinaryDeserializer{T}"/> instance for the type <typeparamref name="T"/>
        /// </summary>
        [Pure]
        private static BinaryDeserializer<T> BuildDeserializer() => _Deserializer.Build(il =>
        {
            // T obj; ...;
            il.DeclareLocal(typeof(T));
            il.DeclareLocalsFromType<Locals.Read>();

            // Initialize T obj to either new T() or null
            il.EmitDeserializeEmptyInstanceOrNull<T>();

            // Skip the deserialization if the instance in null
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Brfalse_S, end);

            // Deserialize all the contained properties
            foreach (PropertyInfo property in
                from prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead && prop.CanWrite
                select prop)
            {
                if (property.PropertyType.IsUnmanaged()) il.EmitDeserializeUnmanagedProperty(property);
                else if (property.PropertyType == typeof(string)) il.EmitDeserializeStringProperty(property);
                else if (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsUnmanaged()) il.EmitDeserializeUnmanagedArrayProperty(property);
                else if (property.PropertyType.IsArray && !property.PropertyType.GetElementType().IsValueType) { } // TODO
                else if (!property.PropertyType.IsValueType)
                {
                    il.EmitLoadLocal(Locals.Read.T);
                    il.EmitLoadArgument(Arguments.Read.Stream);
                    il.EmitCall(OpCodes.Call, KnownMembers.SerializationProcessor.DeserializerInfo(property.PropertyType), null);
                    il.EmitWriteMember(property);
                }
                else throw new InvalidOperationException($"Property of type {property.PropertyType} not supported");
            }

            // return obj;
            il.MarkLabel(end);
            il.EmitLoadLocal(Locals.Read.T);
            il.Emit(OpCodes.Ret);
        });
    }
}
