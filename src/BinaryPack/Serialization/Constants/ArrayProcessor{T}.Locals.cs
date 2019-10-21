﻿using BinaryPack.Attributes;

namespace BinaryPack.Serialization.Processors
{
    internal sealed partial class ArrayProcessor<T>
    {
        /// <summary>
        /// A <see langword="class"/> that exposes hardcoded indices for local variables for <see cref="ArrayProcessor{T}"/>
        /// </summary>
        private static class Locals
        {
            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during serialization
            /// </summary>
            public enum Write
            {
                /// <summary>
                /// The <see cref="byte"/>* local variable
                /// </summary>
                [LocalType(typeof(byte*))]
                BytePtr,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the source <typeparamref name="T"/> array
                /// </summary>
                [LocalType(typeof(int))]
                Length,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I
            }

            /// <summary>
            /// An <see langword="enum"/> with a collection of local variables used during deserialization
            /// </summary>
            public enum Read
            {
                /// <summary>
                /// The target <typeparamref name="T"/> array
                /// </summary>
                ArrayT,

                /// <summary>
                /// The <see cref="byte"/> <see cref="System.Span{T}"/> local variable
                /// </summary>
                [LocalType(typeof(System.Span<byte>))]
                SpanByte,

                /// <summary>
                /// The <see cref="int"/> local variable to track the length of the target <typeparamref name="T"/> array
                /// </summary>
                [LocalType(typeof(int))]
                Length,

                /// <summary>
                /// The <see cref="int"/> local variable for the loop counter
                /// </summary>
                [LocalType(typeof(int))]
                I
            }
        }
    }
}
