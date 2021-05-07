// ************************************************************************************************
// <copyright file="PythonBase64ImageConverter.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Drawing;
using System.IO;

namespace PyRunner
{
	/// <summary>
	/// Helper class for converting a base64 string (as printed by
	/// python script) to a <see cref="Bitmap"/> image.
	/// </summary>
	internal static class PythonBase64ImageConverter
	{
		/// <summary>
		/// Converts a base64 string (as printed by python script) to a <see cref="Bitmap"/> image.
		/// </summary>
		public static Bitmap FromPythonBase64String(string pythonBase64String)
		{
			// Remove the first two chars and the last one.
			// First one is 'b' (python format sign), others are quote signs.
			string base64String = pythonBase64String.Substring(2, pythonBase64String.Length - 3);

			// Convert now raw base46 string to byte array.
			byte[] imageBytes = Convert.FromBase64String(base64String);

			// Read bytes as stream.
			var memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
			memoryStream.Write(imageBytes, 0, imageBytes.Length);

			// Create bitmap from stream.
			return (Bitmap)Image.FromStream(memoryStream, true);
		}
	}
}