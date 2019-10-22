using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BinaryWriter = BinaryPack.Serialization.Buffers.BinaryWriter;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class ArrayTest
    {
        // Test method for a generic arrays of reference types
        public static void Test<T>(T[]? array) where T : class, IEquatable<T>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ArrayProcessor<T>.Instance.Serializer(array, ref writer);
            using Stream stream = new MemoryStream(writer.Span.ToArray());
            T[]? result = ArrayProcessor<T>.Instance.Deserializer(stream);

            // Equality check
            if (array == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(array.Length, result!.Length);
                Assert.IsTrue(array.Zip(result).All(p =>
                {
                    if (p.First == null && p.Second == null) return true;
                    return p.First?.Equals(p.Second) == true;
                }));
            }
        }

        [TestMethod]
        public void ReferenceTypeNullArraySerializationTest() => Test(default(MessagePackSampleModel[]));

        [TestMethod]
        public void ReferenceTypeEmptyArraySerializationTest() => Test(Array.Empty<MessagePackSampleModel>());

        [TestMethod]
        public void ReferenceTypeArraySerializationTest1() => Test(new[] { new MessagePackSampleModel { Compact = true, Schema = 17 } });

        [TestMethod]
        public void ReferenceTypeArraySerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = new MessagePackSampleModel {Compact = compact, Schema = i}
            select model).ToArray());

        [TestMethod]
        public void ReferenceTypeArraySerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let compact = i % 2 == 0
            let model = compact ? null : new MessagePackSampleModel { Compact = compact, Schema = i }
            select model).ToArray());

        [TestMethod]
        public void StringNullArraySerializationTest() => Test(default(string[]));

        [TestMethod]
        public void StringEmptyArraySerializationTest() => Test(Array.Empty<string>());

        [TestMethod]
        public void StringArraySerializationTest1() => Test(new[] { RandomProvider.NextString(60) });

        [TestMethod]
        public void StringArraySerializationTest2() => Test((
            from _ in Enumerable.Range(0, 10)
            select RandomProvider.NextString(60)).ToArray());

        [TestMethod]
        public void StringArraySerializationTest3() => Test((
            from i in Enumerable.Range(0, 10)
            let isNull = i % 2 == 0
            let text = isNull ? null : RandomProvider.NextString(60)
            select text).ToArray());

        // Test method for arrays of an unmanaged type
        public static void Test(DateTime[]? array)
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            ArrayProcessor<DateTime>.Instance.Serializer(array, ref writer);
            using Stream stream = new MemoryStream(writer.Span.ToArray());
            DateTime[]? result = ArrayProcessor<DateTime>.Instance.Deserializer(stream);

            // Equality check
            if (array == null) Assert.IsNull(result);
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(array.Length, result!.Length);
                Assert.IsTrue(MemoryMarshal.AsBytes(array.AsSpan()).SequenceEqual(MemoryMarshal.AsBytes(result.AsSpan())));
            }
        }

        [TestMethod]
        public void UnmanagedTypeNullArraySerializationTest() => Test(default);

        [TestMethod]
        public void UnmanagedTypeEmptyArraySerializationTest() => Test(Array.Empty<DateTime>());

        [TestMethod]
        public void UnmanagedTypeArraySerializationTest1() => Test(new[] { RandomProvider.NextDateTime() });

        [TestMethod]
        public void UnmanagedTypeArraySerializationTest2() => Test((
            from i in Enumerable.Range(0, 10)
            select RandomProvider.NextDateTime()).ToArray());
    }
}