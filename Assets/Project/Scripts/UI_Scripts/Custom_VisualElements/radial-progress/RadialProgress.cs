using Unity.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Library
{
    [UxmlElement]
    public partial class RadialProgress : Game.UI.BoundProgressElementBase
    {
        public static readonly string ussClassName = "radial-progress";
        protected override string controlUssClassName => ussClassName;

        EllipseMesh m_TrackMesh;
        EllipseMesh m_ProgressMesh;

        const int numberOfSteps = 220;

        [UxmlAttribute, CreateProperty]
        public Color progressColor
        {
            get => m_ProgressMesh != null ? m_ProgressMesh.color : default;
            set
            {
                if (m_ProgressMesh == null)
                    return;

                m_ProgressMesh.color = value;
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute, CreateProperty]
        public Color trackColor
        {
            get => m_TrackMesh != null ? m_TrackMesh.color : default;
            set
            {
                if (m_TrackMesh == null)
                    return;

                m_TrackMesh.color = value;
                MarkDirtyRepaint();
            }
        }

        public RadialProgress()
        {
            m_ProgressMesh = new EllipseMesh(numberOfSteps);
            m_TrackMesh = new EllipseMesh(numberOfSteps);

            generateVisualContent += DrawMeshes;
        }

        protected override void OnProgressChanged(float progressPercent)
        {
            // Base already MarkDirtyRepaint() after invoking this hook.
        }

        void DrawMeshes(MeshGenerationContext context)
        {
            if (m_ProgressMesh == null || m_TrackMesh == null)
                return;

            var halfWidth = contentRect.width * 0.5f;
            var halfHeight = contentRect.height * 0.5f;

            if (halfWidth < 2f || halfHeight < 2f)
                return;

            m_ProgressMesh.width = halfWidth;
            m_ProgressMesh.height = halfHeight;
            m_ProgressMesh.borderSize = 10;
            m_ProgressMesh.UpdateMesh();

            m_TrackMesh.width = halfWidth;
            m_TrackMesh.height = halfHeight;
            m_TrackMesh.borderSize = 10;
            m_TrackMesh.UpdateMesh();

            var trackWriteData = context.Allocate(m_TrackMesh.vertices.Length, m_TrackMesh.indices.Length);
            trackWriteData.SetAllVertices(m_TrackMesh.vertices);
            trackWriteData.SetAllIndices(m_TrackMesh.indices);

            var clamped = Mathf.Clamp(GetProgressPercent(), 0f, 100f);
            var sliceSteps = Mathf.FloorToInt((numberOfSteps * clamped) / 100f);

            if (sliceSteps == 0)
                return;

            var sliceIndexCount = sliceSteps * 6;

            var progressWriteData = context.Allocate(m_ProgressMesh.vertices.Length, sliceIndexCount);
            progressWriteData.SetAllVertices(m_ProgressMesh.vertices);

            var temporaryIndices = new NativeArray<ushort>(m_ProgressMesh.indices, Allocator.Temp);
            progressWriteData.SetAllIndices(temporaryIndices.Slice(0, sliceIndexCount));
            temporaryIndices.Dispose();
        }
    }
}
