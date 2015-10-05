﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Grib.Api.Interop;
using System.Text.RegularExpressions;

namespace Grib.Api.Tests
{
    [TestFixture]
    public class Get
    {
        [Test]
        public void TestGetCounts()
        {
            using (GribFile file = new GribFile(Settings.GRIB))
            {
                foreach(var msg in file)
                {
                    Assert.AreNotEqual(msg.DataPointsCount, 0);
                    Assert.AreNotEqual(msg.ValuesCount, 0);
                    Assert.AreEqual(msg.ValuesCount, msg["numberOfCodedValues"].AsInt());
                    Assert.AreEqual(msg.DataPointsCount, msg.ValuesCount + msg.MissingCount);
                }
            }
        }

        [Test]
        public void TestGetVersion()
        {
            Regex re = new Regex(@"^(\d+\.)?(\d+\.)?(\*|\d+)$");
            Assert.IsTrue(re.IsMatch(GribEnvironment.GribApiVersion));
        }

        [Test]
        public void TestGetNativeType()
        {
            using (GribFile file = new GribFile(Settings.REG_LATLON_GRB1))
            {
                var msg = file.First();
                Assert.AreEqual(msg["packingType"].NativeType, GribValueType.String);
                Assert.AreEqual(msg["longitudeOfFirstGridPointInDegrees"].NativeType, GribValueType.Double);
                Assert.AreEqual(msg["numberOfPointsAlongAParallel"].NativeType, GribValueType.Int);
                Assert.AreEqual(msg["values"].NativeType, GribValueType.DoubleArray);

                // TODO: test other types
            }
        }

        [Test]
        public void TestCanConvertToDegress ()
        {
            using (GribFile file = new GribFile(Settings.REDUCED_LATLON_GRB2))
            {
                var msg = file.First();

                // true
                Assert.IsTrue(msg["latitudeOfFirstGridPointInDegrees"].CanConvertToDegrees);
                Assert.IsTrue(msg["latitudeOfFirstGridPoint"].CanConvertToDegrees);
                Assert.IsTrue(msg["longitudeOfFirstGridPointInDegrees"].CanConvertToDegrees);
                Assert.IsTrue(msg["longitudeOfFirstGridPoint"].CanConvertToDegrees);
                Assert.IsTrue(msg["latitudeOfLastGridPointInDegrees"].CanConvertToDegrees);
                Assert.IsTrue(msg["latitudeOfLastGridPoint"].CanConvertToDegrees);
                Assert.IsTrue(msg["jDirectionIncrement"].CanConvertToDegrees);
                Assert.IsTrue(msg["iDirectionIncrement"].CanConvertToDegrees);

                // false
                Assert.IsFalse(msg["numberOfPointsAlongAParallel"].CanConvertToDegrees);
                Assert.IsFalse(msg["numberOfPointsAlongAParallelInDegrees"].CanConvertToDegrees);
                Assert.IsFalse(msg["numberOfPointsAlongAMeridian"].CanConvertToDegrees);
                Assert.IsFalse(msg["numberOfPointsAlongAMeridianInDegrees"].CanConvertToDegrees);
                Assert.IsFalse(msg["packingType"].CanConvertToDegrees);
            }
        }

        [Test]
        public void TestGetGrib2 ()
        {
            double delta = 0.1d;

            using (GribFile file = new GribFile(Settings.REDUCED_LATLON_GRB2))
            {
                var msg = file.First();

                // "InDegrees" is a magic token that converts coordinate double values to degrees
                // explicit degree conversion via key name
                double latitudeOfFirstGridPointInDegrees = msg["latitudeOfFirstGridPoint"].AsDouble();
                Assert.AreEqual(latitudeOfFirstGridPointInDegrees, 90, delta);

                // degree conversion via accessor
                double longitudeOfFirstGridPointInDegrees = msg["longitudeOfFirstGridPoint"].AsDouble(/* inDegrees == true */);
                Assert.AreEqual(longitudeOfFirstGridPointInDegrees, 0, delta);

                // degree conversion via accessor
                double latitudeOfLastGridPointInDegrees = msg["latitudeOfLastGridPoint"].AsDouble(/* inDegrees == true */);
                Assert.AreEqual(latitudeOfLastGridPointInDegrees, -90, delta);

                // degree conversion via accessor
                double longitudeOfLastGridPointInDegrees = msg["longitudeOfLastGridPoint"].AsDouble(/* inDegrees == true */);
                Assert.AreEqual(longitudeOfLastGridPointInDegrees, 360, .5);

                // degree conversion via accessor
                double jDirectionIncrementInDegrees = msg["jDirectionIncrement"].AsDouble(/* inDegrees == true */);
                Assert.AreEqual(jDirectionIncrementInDegrees, 0.36, delta);

                // degree conversion via accessor
                double iDirectionIncrementInDegrees = msg["iDirectionIncrement"].AsDouble(/* inDegrees == true */);
                Assert.AreEqual(iDirectionIncrementInDegrees, -1.0E+100, delta);

                int numberOfPointsAlongAParallel = msg["numberOfPointsAlongAParallel"].AsInt();
                Assert.AreEqual(numberOfPointsAlongAParallel, -1);

                int numberOfPointsAlongAMeridian = msg["numberOfPointsAlongAMeridian"].AsInt();
                Assert.AreEqual(numberOfPointsAlongAMeridian, 501);

                string packingType = msg["packingType"].AsString();
                Assert.AreEqual("grid_simple", packingType);             
            }
        }
    }
}
