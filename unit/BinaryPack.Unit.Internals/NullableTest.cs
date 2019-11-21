﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BinaryPack.Models;
using BinaryPack.Models.Helpers;
using BinaryPack.Serialization.Buffers;
using BinaryPack.Serialization.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryPack.Unit.Internals
{
    [TestClass]
    public class NullableTest
    {
        // Test method for nullable struct types
        private static void Test<T>(T? value) where T : struct, IEquatable<T>
        {
            // Serialization
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            NullableProcessor<T>.Instance.Serializer(value, ref writer);
            Span<byte> span = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(writer.Span.GetPinnableReference()), writer.Span.Length);
            BinaryReader reader = new BinaryReader(span);
            T? result = NullableProcessor<T>.Instance.Deserializer(ref reader);

            // Equality check
            Assert.IsTrue(StructuralComparer.IsMatch(value, result));
        }

        [TestMethod]
        public void NullableBool1() => Test<bool>(null);
        [TestMethod]
        public void NullableBool2() => Test<bool>(true);
        [TestMethod]
        public void NullableBool3() => Test<bool>(false);

        [TestMethod]
        public void NullableInt1() => Test<int>(null);

        [TestMethod]
        public void NullableInt2() => Test<int>(77);

        [TestMethod]
        public void NullableDateTime1() => Test<DateTime>(null);

        [TestMethod]
        public void NullableDateTime2() => Test<DateTime>(DateTime.Now);

        [TestMethod]
        public void NullableManagedValueType1() => Test<ValidationValueTypeModel>(null);

        [TestMethod]
        public void NullableManagedValueType2()
        {
            ValidationValueTypeModel? model = new ValidationValueTypeModel();
            model.Value.Initialize();

            Test(model);
        }
    }
}
