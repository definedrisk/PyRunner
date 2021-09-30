// <copyright file="PythonBase64ImageConverter.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX
{
    using System;

    using SixLabors.ImageSharp;

    /// <summary>
    /// Helper class for converting a base64 string (as printed by
    /// python script) to a <see cref="Image"/> image.
    /// </summary>
    internal static class PythonBase64ImageConverter
    {
        /// <summary>
        /// Converts a base64 string (as printed by python script) to an ImageSharp <see cref="Image"/>.
        /// </summary>
        public static Image FromPythonBase64String(string pythonBase64String)
        {
            // Remove the first two chars and the last one.
            // First one is 'b' (python format sign), others are quote signs.
            string base64String = pythonBase64String[2..^1];

            // Convert now raw base64 string to byte array.
            byte[] imageBytes = Convert.FromBase64String(base64String);

            ////// Read bytes as stream.
            ////var memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ////memoryStream.Write(imageBytes, 0, imageBytes.Length);

            // Create image from stream.
            return Image.Load(imageBytes);
        }
    }
}
