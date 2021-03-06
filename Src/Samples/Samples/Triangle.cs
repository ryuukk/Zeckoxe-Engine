﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Zeckoxe.Core;
using Zeckoxe.Desktop;
using Zeckoxe.Engine;
using Zeckoxe.Games;
using Zeckoxe.Graphics;
using Zeckoxe.Graphics.Toolkit;
using Zeckoxe.Physics;
using Zeckoxe.ShaderCompiler;
using Buffer = Zeckoxe.Graphics.Buffer;

namespace Samples.Samples
{
    public class Triangle : Game, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TransformUniform
        {
            public TransformUniform(Matrix4x4 p, Matrix4x4 m, Matrix4x4 v)
            {
                P = p;
                M = m;
                V = v;
            }

            public Matrix4x4 P;

            public Matrix4x4 M;

            public Matrix4x4 V;

            public void Update(Camera camera, Matrix4x4 m)
            {
                P = camera.Projection;
                M = m;
                V = camera.View;
            }
        }

        public Triangle() : base()
        {

        }

        public Camera Camera { get; set; }
        public GraphicsPipelineState PipelineState { get; set; }
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }
        public Buffer ConstBuffer { get; set; }

        public DescriptorSet Descriptor { get; set; }

        public TransformUniform Uniform;



        public override void InitializeSettings()
        {
            base.InitializeSettings();
            Parameters.Settings.Validation = ValidationType.None;
            Window.Title += " - (Triangle) ";
        }

        public override void Initialize()
        {
            base.Initialize();


            Camera = new()
            {
                Mode = CameraType.Free,
                Position = new(0, 0, -2.5f),
            };

            Camera.SetLens(Window.Width, Window.Height);


            // Reset Model
            Model = Matrix4x4.Identity;
            Models.Add(Model);


            Uniform = new(Camera.Projection, Model, Camera.View);

            CreateBuffers();

            CreatePipelineState();



            Descriptor = new(PipelineState);
            Descriptor.SetUniformBuffer(0, ConstBuffer); // Binding 0: Uniform buffer (Vertex shader)Descriptor
            Descriptor.Build();
        }




        public void CreateBuffers()
        {

            VertexPositionColor[] vertices = new[]
            {
                new VertexPositionColor(new Vector3(0.0f, -0.65f, -0.5f), new Vector3(1.6f, 0.0f, 0.0f)),
                new VertexPositionColor(new Vector3(0.65f, 0.65f, -0.5f), new Vector3(0.0f, 1.6f, 0.0f)),
                new VertexPositionColor(new Vector3(-0.65f, 0.65f, -0.5f), new Vector3(0.0f, 0.0f, 1.6f)),
            };


            int[] indices = new[]
            {
                0, 1, 2
            };


            VertexBuffer = new(Device, new()
            {
                BufferFlags = BufferFlags.VertexBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf<VertexPositionColor>(vertices),
            });
            VertexBuffer.SetData(vertices);


            IndexBuffer = new(Device, new()
            {
                BufferFlags = BufferFlags.IndexBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf<int>(indices),
            });
            IndexBuffer.SetData(indices);


            ConstBuffer = new(Device, new()
            {
                BufferFlags = BufferFlags.ConstantBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf<TransformUniform>(),
            });


        }


        public void CreatePipelineState()
        {
            PipelineStateDescription Pipelinedescription = new()
            {
                Framebuffer = Framebuffer,

                Layouts =
                {
                    // Binding 0: Uniform buffer (Vertex shader)
                    new()
                    {
                        Stage = ShaderStage.Vertex,
                        Type = DescriptorType.UniformBuffer,
                        Binding = 0,
                    }
                },

                InputAssemblyState = new()
                {
                    PrimitiveType = PrimitiveType.TriangleList,
                },
                RasterizationState = new()
                {
                    FillMode = FillMode.Solid,
                    CullMode = CullMode.Front,
                    FrontFace = FrontFace.Clockwise
                },
                PipelineVertexInput = new()
                {
                    VertexAttributeDescriptions =
                    {
                        new()
                        {
                            Binding = 0,
                            Location = 0,
                            Format = PixelFormat.R32G32B32SFloat,
                            Offset = 0,
                        },
                        new()
                        {
                            Binding = 0,
                            Location = 1,
                            Format = PixelFormat.R32G32B32SFloat,
                            Offset = 12,
                        }
                    },
                    VertexBindingDescriptions =
                    {
                        new()
                        {
                            Binding = 0,
                            InputRate = VertexInputRate.Vertex,
                            Stride = VertexPositionColor.Size,
                        }
                    },
                },
                Shaders = 
                {
                    ShaderBytecode.LoadFromFile("Shaders/PositionColor/shader.frag", ShaderStage.Fragment),
                    ShaderBytecode.LoadFromFile("Shaders/PositionColor/shader.vert", ShaderStage.Vertex),
                },
            };

            PipelineState = new(Pipelinedescription);
        }


        public override void Update(GameTime game)
        {
            Camera.Update(game);



            Uniform.Update(Camera, Model);

            ConstBuffer.SetData(ref Uniform);
        }


        public override void BeginDraw()
        {
            base.BeginDraw();

            CommandBuffer commandBuffer = Context.CommandBuffer;


            commandBuffer.BeginFramebuffer(Framebuffer);

            commandBuffer.SetViewport(Window.Width, Window.Height, 0, 0);
            commandBuffer.SetScissor(Window.Width, Window.Height, 0, 0);

            commandBuffer.SetGraphicPipeline(PipelineState);
            commandBuffer.SetVertexBuffers(new Buffer[] { VertexBuffer });
            commandBuffer.SetIndexBuffer(IndexBuffer);
            commandBuffer.BindDescriptorSets(Descriptor);
            commandBuffer.DrawIndexed(3, 1, 0, 0, 0);
        }



        public void Dispose()
        {
            Adapter.Dispose();
        }
    }
}
