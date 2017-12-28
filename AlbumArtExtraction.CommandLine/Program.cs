using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace AlbumArtExtraction.CommandLine {
	class Program {
		static void Main(string[] args) {
			var optionArgs =
				from arg in args
				where arg.StartsWith("-")
				select arg.Substring(1);

			var mainArgs =
				from arg in args
				where !arg.StartsWith("-")
				select arg;

			// a value indicating whether to overwrite
			var noConfirm = optionArgs.FirstOrDefault(i => i == "y") != null;

			string inputPath, outputPath;

			// inputPath
			if (mainArgs.Count() == 1) {
				inputPath = mainArgs.ElementAt(0);
				outputPath = Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath));
			}
			// inputPath, outputPath
			else if (mainArgs.Count() == 2) {
				inputPath = mainArgs.ElementAt(0);
				outputPath = mainArgs.ElementAt(1);
			}
			else {
				Usage();
				return;
			}

			try {
				var selector = new Selector();
				var extractor = selector.SelectAlbumArtExtractor(inputPath);
				Console.WriteLine($"selected extractor: {extractor}");

				using (var albumArt = extractor.Extract(inputPath)) {
					var format = albumArt.RawFormat;
					outputPath += (format == ImageFormat.Png) ? ".png" : ".jpg";
					if (!noConfirm && File.Exists(outputPath)) {
						Console.Write("file name already exists. do you want to overwrite it? (y/n) ");
						var input = Console.ReadLine();
						if (!input.ToLower().StartsWith("y")) {
							return;
						}
					}
					albumArt.Save(outputPath);
				}

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"file creation succeeded: {outputPath}");
				Console.ResetColor();
			}
			catch(FileNotFoundException) {
				Error(() => Console.WriteLine($"input file is not found: {inputPath}"));
			}
			catch(NotSupportedException ex) {
				Error(() => {
					Console.WriteLine($"format of input file is not supported:");
					Console.WriteLine(ex);
				});
			}
			catch(Exception ex) {
				Error(() => {
					Console.WriteLine($"error:");
					Console.WriteLine(ex);
				});
			}
		}

		static void Error(Action content) {
			Console.ForegroundColor = ConsoleColor.Red;
			content();
			Console.ResetColor();
		}

		static void Usage() {
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.WriteLine();
			Console.WriteLine("[-y] inputPath [ outputPath ]");
			Console.WriteLine();
			Console.WriteLine("-y : Overwrite existing file");
		}
	}
}
