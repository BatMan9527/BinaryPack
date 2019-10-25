﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BinaryPack.Serialization.Constants;
using BinaryPack.Serialization.Processors.Abstract;
using BinaryPack.Serialization.Reflection;

namespace BinaryPack.Serialization.Processors
{
    /// <summary>
    /// A <see langword="class"/> responsible for creating the serializers and deserializers for <see cref="Dictionary{K,V}"/> types
    /// </summary>
    /// <typeparam name="K">The type of the keys in the dictionary to serialize and deserialize</typeparam>
    /// <typeparam name="V">The type of the values in the dictionary to serialize and deserialize</typeparam>
    internal sealed partial class DictionaryProcessor<K, V> : TypeProcessor<Dictionary<K, V>?>
    {
        /// <summary>
        /// The <see cref="Type"/> instance for the nested <see cref="Dictionary{TKey,TValue}"/>.Entry <see langword="struct"/>
        /// </summary>
        private static readonly Type EntryType = typeof(Dictionary<K, V>).GetNestedType("Entry", BindingFlags.NonPublic);

        /// <summary>
        /// Gets the singleton <see cref="DictionaryProcessor{K,V}"/> instance to use
        /// </summary>
        public static DictionaryProcessor<K, V> Instance { get; } = new DictionaryProcessor<K, V>();

        /// <inheritdoc/>
        protected override void EmitSerializer(ILGenerator il)
        {
            il.DeclareLocals<Locals.Write>();
            il.DeclareLocal(EntryType.MakeByRefType());

            //int count = obj?.Count ?? - 1;
            Label
                isNotNull = il.DefineLabel(),
                countLoaded = il.DefineLabel();
            il.EmitLoadArgument(Arguments.Write.T);
            il.Emit(OpCodes.Brtrue_S, isNotNull);
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Br_S, countLoaded);
            il.MarkLabel(isNotNull);
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(Dictionary<K, V>).GetProperty(nameof(Dictionary<K, V>.Count)));
            il.MarkLabel(countLoaded);
            il.EmitStoreLocal(Locals.Write.Count);

            // writer.Write(count);
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitLoadLocal(Locals.Write.Count);
            il.EmitCall(KnownMembers.BinaryWriter.WriteT(typeof(int)));

            // if (count > 0) { }
            Label end = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.Count);
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ble_S, end);

            // ref Entry r0 = ref obj._entries[0];
            il.EmitLoadArgument(Arguments.Write.T);
            il.EmitReadMember(typeof(Dictionary<K, V>).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance));
            il.EmitLoadInt32(0);
            il.Emit(OpCodes.Ldelema, EntryType);
            il.EmitStoreLocal(Locals.Write.RefEntry);

            // for (int i = 0; i < count;;) { }
            Label check = il.DefineLabel();
            il.EmitLoadInt32(0);
            il.EmitStoreLocal(Locals.Write.I);
            il.Emit(OpCodes.Br_S, check);
            Label loop = il.DefineLabel();
            il.MarkLabel(loop);

            // if (r0.next >= -1) { }
            Label emptyEntry = il.DefineLabel();
            il.EmitLoadLocal(Locals.Write.RefEntry);
            il.EmitReadMember(EntryType.GetField("next"));
            il.EmitLoadInt32(-1);
            il.Emit(OpCodes.Blt_S, emptyEntry);

            // TypeProcessor<T>.Serializer(r0.key, ref writer);
            il.EmitLoadLocal(Locals.Write.RefEntry);
            il.EmitReadMember(EntryType.GetField("key"));
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(K)));

            // TypeProcessor<T>.Serializer(r0.value, ref writer);
            il.EmitLoadLocal(Locals.Write.RefEntry);
            il.EmitReadMember(EntryType.GetField("value"));
            il.EmitLoadArgument(Arguments.Write.RefBinaryWriter);
            il.EmitCall(KnownMembers.TypeProcessor.SerializerInfo(typeof(V)));

            il.MarkLabel(emptyEntry);

            // r0 = ref Unsafe.Add(ref r0, 1);
            il.EmitLoadLocal(Locals.Write.RefEntry);
            il.Emit(OpCodes.Sizeof, EntryType);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.EmitStoreLocal(Locals.Write.RefEntry);

            // i++
            il.MarkLabel(check);
            il.EmitLoadLocal(Locals.Write.I);
            il.EmitLoadLocal(Locals.Write.Count);
            il.Emit(OpCodes.Blt_S, loop);

            // return;
            il.MarkLabel(end);
            il.Emit(OpCodes.Ret);
        }

        /// <inheritdoc/>
        protected override void EmitDeserializer(ILGenerator il)
        {
            il.Emit(OpCodes.Ret); // TODO
        }
    }
}
