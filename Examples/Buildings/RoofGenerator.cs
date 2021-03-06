using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralToolkit.Examples
{
    public static class RoofGenerator
    {
        private const float GabledRoofHeight = 2;
        private const float HippedRoofHeight = 2;

        public static MeshDraft Generate(
            List<Vector2> foundationPolygon,
            float roofHeight,
            RoofConfig roofConfig)
        {
            List<Vector2> roofPolygon = Geometry.OffsetPolygon(foundationPolygon, roofConfig.overhang);

            MeshDraft roofDraft;
            switch (roofConfig.type)
            {
                case RoofType.Flat:
                    roofDraft = GenerateFlat(roofPolygon, roofConfig);
                    break;
                case RoofType.Gabled:
                    roofDraft = GenerateGabled(roofPolygon, roofConfig);
                    break;
                case RoofType.Hipped:
                    roofDraft = GenerateHipped(roofPolygon, roofConfig);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (roofConfig.thickness > 0)
            {
                roofDraft.Add(GenerateBorder(roofPolygon, roofConfig));
            }

            if (roofConfig.overhang > 0)
            {
                roofDraft.Add(GenerateOverhang(foundationPolygon, roofPolygon));
            }

            roofDraft.Move(Vector3.up*roofHeight);
            roofDraft.name = "Roof";
            return roofDraft;
        }

        private static MeshDraft GenerateFlat(List<Vector2> roofPolygon, RoofConfig roofConfig)
        {
            Vector3 a = roofPolygon[0].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 b = roofPolygon[3].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 c = roofPolygon[2].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 d = roofPolygon[1].ToVector3XZ() + Vector3.up*roofConfig.thickness;

            var roofDraft = new MeshDraft();
            roofDraft.AddQuad(a, d, c, b, Vector3.up);
            return roofDraft;
        }

        public static MeshDraft GenerateGabled(List<Vector2> roofPolygon, RoofConfig roofConfig)
        {
            Vector3 a = roofPolygon[0].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 b = roofPolygon[3].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 c = roofPolygon[2].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 d = roofPolygon[1].ToVector3XZ() + Vector3.up*roofConfig.thickness;

            Vector3 ridgeHeight = Vector3.up*GabledRoofHeight;
            Vector3 ridge0 = (a + d)/2 + ridgeHeight;
            Vector3 ridge1 = (b + c)/2 + ridgeHeight;

            var roofDraft = new MeshDraft();
            roofDraft.AddQuad(a, ridge0, ridge1, b);
            roofDraft.AddTriangle(b, ridge1, c);
            roofDraft.AddQuad(c, ridge1, ridge0, d);
            roofDraft.AddTriangle(d, ridge0, a);
            return roofDraft;
        }

        public static MeshDraft GenerateHipped(List<Vector2> roofPolygon, RoofConfig roofConfig)
        {
            Vector3 a = roofPolygon[0].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 b = roofPolygon[3].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 c = roofPolygon[2].ToVector3XZ() + Vector3.up*roofConfig.thickness;
            Vector3 d = roofPolygon[1].ToVector3XZ() + Vector3.up*roofConfig.thickness;

            Vector3 ridgeHeight = Vector3.up*HippedRoofHeight;
            Vector3 ridgeOffset = (b - a).normalized*2;
            Vector3 ridge0 = (a + d)/2 + ridgeHeight + ridgeOffset;
            Vector3 ridge1 = (b + c)/2 + ridgeHeight - ridgeOffset;

            var roofDraft = new MeshDraft();
            roofDraft.AddQuad(a, ridge0, ridge1, b);
            roofDraft.AddTriangle(b, ridge1, c);
            roofDraft.AddQuad(c, ridge1, ridge0, d);
            roofDraft.AddTriangle(d, ridge0, a);
            return roofDraft;
        }

        private static MeshDraft GenerateBorder(List<Vector2> roofPolygon, RoofConfig roofConfig)
        {
            List<Vector3> lowerRing = roofPolygon.ConvertAll(v => v.ToVector3XZ());
            List<Vector3> upperRing = roofPolygon.ConvertAll(v => v.ToVector3XZ() + Vector3.up*roofConfig.thickness);
            return new MeshDraft().AddFlatQuadBand(lowerRing, upperRing, false);
        }

        private static MeshDraft GenerateOverhang(List<Vector2> foundationPolygon, List<Vector2> roofPolygon)
        {
            List<Vector3> lowerRing = foundationPolygon.ConvertAll(v => v.ToVector3XZ());
            List<Vector3> upperRing = roofPolygon.ConvertAll(v => v.ToVector3XZ());
            return new MeshDraft().AddFlatQuadBand(lowerRing, upperRing, false);
        }
    }

    public enum RoofType
    {
        Flat,
        Hipped,
        Gabled,
    }

    [Serializable]
    public class RoofConfig
    {
        public RoofType type = RoofType.Flat;
        public float thickness;
        public float overhang;
    }
}
