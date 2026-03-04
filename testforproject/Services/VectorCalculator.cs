using System;
using System.Collections.Generic;
using System.Linq;

namespace testforproject.Services
{
    public class VectorCalculator
    {
        // 1. Calculate Dot Product (ผลคูณเชิงสเกลาร์)
        // สูตร: sum(u_i * v_i)
        public double CalculateDotProduct(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
            {
                throw new ArgumentException("Vectors must be of the same length.");
            }

            double dotProduct = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
            }
            return dotProduct;
        }

        // 2. Calculate Magnitude (ความยาวของเวกเตอร์)
        // สูตร: sqrt(sum(v_i ^ 2))
        public double CalculateMagnitude(double[] vector)
        {
            double sumOfSquares = 0;
            foreach (var value in vector)
            {
                sumOfSquares += Math.Pow(value, 2);
            }
            return Math.Sqrt(sumOfSquares);
        }

        // 3. Calculate Cosine Similarity (ความคล้ายคลึงรูปลิ่ม)
        // สูตร: DotProduct(A, B) / (Magnitude(A) * Magnitude(B))
        public double CalculateCosineSimilarity(double[] vectorA, double[] vectorB)
        {
            double dotProduct = CalculateDotProduct(vectorA, vectorB);
            double magnitudeA = CalculateMagnitude(vectorA);
            double magnitudeB = CalculateMagnitude(vectorB);

            // Safety Rule: ป้องกันเหตุการณ์ Divide by Zero เสี้ยววินาทีที่รัน
            // ถ้าเวกเตอร์ใดเวกเตอร์หนึ่งมีค่าเป็น 0 ทั้งตาราง (ความยาว = 0)
            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0.0; // ถือว่าไม่มีความเหมือนกันเลย (0%) ดักแครชระบบ
            }

            return dotProduct / (magnitudeA * magnitudeB);
        }
    }
}
