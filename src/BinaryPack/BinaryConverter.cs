﻿using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.IO;
using BinaryPack.Serialization;

namespace BinaryPack
{
    /// <summary>
    /// The entry point <see langword="class"/> for all the APIs in the library
    /// </summary>
    public static class BinaryConverter
    {
        /// <summary>
        /// Serializes the input <typeparamref name="T"/> instance and returns a <see cref="Memory{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to serialize</typeparam>
        /// <param name="obj">The input instance to serialize</param>
        /// <returns>A <see cref="Memory{T}"/> instance containing the serialized data</returns>
        public static Memory<byte> Serialize<T>(T obj) where T : new()
        {
            using MemoryStream stream = new MemoryStream();
            Serialize(obj, stream);
            byte[] data = stream.GetBuffer();

            return new Memory<byte>(data, 0, (int)stream.Position);
        }

        /// <summary>
        /// Serializes the input <typeparamref name="T"/> instance to the target <see cref="Stream"/>
        /// </summary>
        /// <typeparam name="T">The type of instance to serialize</typeparam>
        /// <param name="obj">The input instance to serialize</param>
        /// <param name="stream">The <see cref="Stream"/> instance to use to write the data</param>
        public static void Serialize<T>(T obj, Stream stream) where T : new()
        {
            SerializationProcessor<T>.Serializer(obj, stream);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Memory{T}"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="memory">The input <see cref="Memory{T}"/> instance to read data from</param>
        [Pure]
        public static unsafe T Deserialize<T>(Memory<byte> memory) where T : new()
        {
            using MemoryHandle handle = memory.Pin();
            using UnmanagedMemoryStream stream = new UnmanagedMemoryStream((byte*)handle.Pointer, memory.Length);

            return Deserialize<T>(stream);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> instance from the input <see cref="Stream"/> instance
        /// </summary>
        /// <typeparam name="T">The type of instance to deserialize</typeparam>
        /// <param name="stream">The input <see cref="Stream"/> instance to read data from</param>
        [Pure]
        public static T Deserialize<T>(Stream stream) where T : new()
        {
            return SerializationProcessor<T>.Deserializer(stream);
        }
    }
}
