namespace KianCommons.StockCode {
    using ColossalFramework.Math;
    using UnityEngine;

    class PassengerCarAI {
        private static ushort CheckOverlap(ushort ignoreParked, ref Bezier3 bezier,
            Vector3 pos, Vector3 dir, float offset, float length,
            ushort otherID, ref VehicleParked otherData,
            ref bool overlap, ref float minPos, ref float maxPos)
        {
            if (otherID != ignoreParked) {
                VehicleInfo info = otherData.Info;
                Vector3 otherPos = otherData.m_position;
                Vector3 diff = otherPos - pos;
                float carLength = info.m_generatedInfo.m_size.z;
                float lengthAvr = (length + carLength) * 0.5f + 1f;
                float diffLength = diff.magnitude;
                if (diffLength < lengthAvr - 0.5f) {
                    overlap = true;
                    float l1;
                    float l2;
                    if (Vector3.Dot(diff, dir) >= 0f) {
                        l1 = lengthAvr + diffLength;
                        l2 = lengthAvr - diffLength;
                    } else {
                        l1 = lengthAvr - diffLength;
                        l2 = lengthAvr + diffLength;
                    }
                    maxPos = Mathf.Max(maxPos, bezier.Travel(offset, l1));
                    minPos = Mathf.Min(minPos, bezier.Travel(offset, -l2));
                }
            }
            return otherData.m_nextGridParked;
        }
    }
}
