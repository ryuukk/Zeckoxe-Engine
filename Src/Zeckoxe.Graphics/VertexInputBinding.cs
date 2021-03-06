﻿// Copyright (c) 2019-2020 Faber Leonardo. All Rights Reserved. https://github.com/FaberSanZ
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

/*=============================================================================
	VertexInputBinding.cs
=============================================================================*/

namespace Zeckoxe.Graphics
{
    public class VertexInputBinding
    {
        public VertexInputBinding()
        {

        }

        public int Binding { get; set; }
        public int Stride { get; set; }
        public VertexInputRate InputRate { get; set; }
    }
}
