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

            float[] f = new float[100];
            float[] v = new float[100];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = i / 100f;
                v[i] = Curves.hillCurve.Evaluate(f[i]);
            }

        }

        [TestMethod]
        public void SinHillCurve()
        {

            float[] f = new float[100];
            float[] v = new float[100];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = i / 100f;
                v[i] = Curves.sinHill.Evaluate(f[i]);
                Console.WriteLine(i + " " + v[i]);
            }

        }
    }
}
