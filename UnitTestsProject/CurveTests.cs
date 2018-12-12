using System;
using KinectBodyModification;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestsProject
{
    [TestClass]
    public class CurveTests
    {
        [TestMethod]
        public void HillCurve()
        {
            var f = new float[100];
            var v = new float[100];
            for (var i = 0; i < f.Length; i++)
            {
                f[i] = i / 100f;
                v[i] = Curves.legsCurve.Evaluate(f[i]);
            }
        }

        [TestMethod]
        public void SinHillCurve()
        {
            var f = new float[100];
            var v = new float[100];
            for (var i = 0; i < f.Length; i++)
            {
                f[i] = i / 100f;
                v[i] = Curves.sinHill.Evaluate(f[i]);
                Console.WriteLine(i + " " + v[i]);
            }
        }
    }
}