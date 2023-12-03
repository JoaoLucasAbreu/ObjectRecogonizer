using SkiaSharp;

namespace Projeto {
	class Program {
		static void Main(string[] args) {
			
			Console.WriteLine("Número da cena: ");
			int cena = Console.ReadLine();

			Console.WriteLine("Quantidade de quadros na cena: ");
			int quadros = Console.ReadLine();

			for (int q = 0; q < quadros; q++)
			{
				using (
					SKBitmap bitmapEntrada = SKBitmap.Decode($"img\\raw\\Cena{cena}_{q}.png"),
					bitmapSaida = new SKBitmap(new SKImageInfo(bitmapEntrada.Width, bitmapEntrada.Height, SKColorType.Gray8))) {

					unsafe {
						byte* entrada = (byte*)bitmapEntrada.GetPixels();
						byte* saida = (byte*)bitmapSaida.GetPixels();

						long pixelsTotais = bitmapEntrada.Width * bitmapEntrada.Height;
						long media = 0;

						// RGB para escala de cinza
						for (int e = 0, s = 0; s < pixelsTotais; e += 4, s++) {
							saida[s] = (byte)((entrada[e] + entrada[e + 1] + entrada[e + 2]) / 3);
							media += saida[s];
						}

						media = (byte)(media / pixelsTotais);

						// Limiarização
						for(int p = 0; p < pixelsTotais; p++)
						{
							if(saida[p] > media)
								saida[p] = 0;
							else
								saida[p] = 255;
						}

						// Erosão
						Erodir(saida, bitmapSaida.Width, bitmapSaida.Height, 7, entrada);
					}

					using (FileStream stream = new FileStream("img\\erosed\\Gabarito Correto Erosed 1.png", FileMode.OpenOrCreate, FileAccess.Write)) {
						bitmapSaida.Encode(stream, SKEncodedImageFormat.Png, 100);
					}
				}
			}










			using (
				SKBitmap bitmapEntrada = SKBitmap.Decode("img\\empty\\Gabarito Correto Vazio Cinza.png"),
				bitmapSaida = new SKBitmap(new SKImageInfo(bitmapEntrada.Width, bitmapEntrada.Height, SKColorType.Gray8))) {
				
				unsafe {
					byte* entrada = (byte*)bitmapEntrada.GetPixels();
					byte* saida = (byte*)bitmapSaida.GetPixels();

					long pixelsTotais = bitmapEntrada.Width * bitmapEntrada.Height;


					for (int i = 0; i < pixelsTotais; i++)
					{
						saida[i] = entrada[i];
					}

					// Limiarização
					for(int p = 0; p < pixelsTotais; p++)
					{
						if(saida[p] > 122)
							saida[p] = 0;
						else
							saida[p] = 255;
					}

					var formas = Forma.DetectarFormas(saida, bitmapSaida.Width, bitmapSaida.Height, true);
					int count = 1;
					int row = 1;
					foreach (var forma in formas){
						if ((forma.Altura == 47 || forma.Altura == 48) && (forma.Largura == 47 || forma.Largura == 48))
						{
							string answer = "";
							switch(count){
								case 1:
									answer = "A";
									count++;
									break;
								case 2:
									answer = "B";
									count++;
									break;
								case 3:
									answer = "C";
									count++;
									break;
								case 4:
									answer = "D";
									count++;
									break;
								case 5:
									answer = "E";
									count = 1;
									break;
							}

							slotsAlternativas.Add((x0: forma.X0, y0: forma.Y0, x1: forma.X1, y1: forma.Y1, row, answer));
							if (count == 1){
								row++;
							}
						}
					}
				}

				using (FileStream stream = new FileStream("img\\erosed\\Gabarito Correto Vazio Cinza Limi.png", FileMode.OpenOrCreate, FileAccess.Write)) {
					bitmapSaida.Encode(stream, SKEncodedImageFormat.Png, 100);
				}
			}

			using (
				SKBitmap bitmapEntrada = SKBitmap.Decode("img\\erosed\\Gabarito Correto Erosed 1.png"),
				bitmapSaida = new SKBitmap(new SKImageInfo(bitmapEntrada.Width, bitmapEntrada.Height, SKColorType.Gray8))) {

				unsafe {
					byte* entrada = (byte*)bitmapEntrada.GetPixels();
					var formas = Forma.DetectarFormas(entrada, bitmapEntrada.Width, bitmapEntrada.Height, true);

					List<(int x0, int y0 , int x1 ,int y1)> quadrados = new List<(int x0, int y0 , int x1 ,int y1)> 
					{
						(32, 32, 82, 82),
						(639, 32, 689, 82),
						(32, 496, 82, 545),
						(639, 496, 689, 545),
						(32, 956, 82, 1009),
						(639, 956, 689, 1009)
					};

					int count = 0;
					for (int i = 0; i < formas.Count(); i++){
						foreach(var quadrado in quadrados){
							if(formas[i].FazInterseccao(quadrado.x0, quadrado.y0, quadrado.x1, quadrado.y1))
								count++;
						}
					}

					// Validação dos cantos 
					if (count == 6) {
						Console.WriteLine("RESULTADO DO GABARITO:" + "\n------------------------");
						int row = 0;
						int c  = 0;
						foreach(var alternativa in slotsAlternativas){
							if(row != alternativa.number){
								if(row != 0 && c == 0){
									Console.Write("Nenhuma Alternativa");
								}
								c = 0;
								row = alternativa.number;
								Console.Write("\n" + row + ": ");
							}

							foreach(var forma in formas){
								if(forma.FazInterseccao(alternativa.x0, alternativa.y0, alternativa.x1, alternativa.y1)){
									Console.Write(alternativa.answer + " ");
									c++;
									break;
								}
							}
						}

					} else {
						Console.WriteLine("Gabarito Errado");
					}
				}
			}
		}

		static unsafe void Erodir(byte* imagem, int largura, int altura, int tamanhoJanela, byte* temp) {
			if ((tamanhoJanela & 1) == 0) {
				throw new Exception("O tamanho deve ser um valor ímpar");
			}

			if (tamanhoJanela < 3) {
				throw new Exception("O tamanho deve ser >= 3");
			}

			int metade = tamanhoJanela >> 1;

			// Primeira passada (vertical)
			for (int y = 0; y < altura; y++) {
				for (int x = 0; x < largura; x++) {
					int valor = 255;

					int yInicial = y - metade;
					int tamanhoValido = tamanhoJanela;
					if (yInicial < 0) {
						tamanhoValido = tamanhoValido + yInicial;
						yInicial = 0;
					}
					if ((y + metade) > (altura - 1)) {
						tamanhoValido = tamanhoValido - ((y + metade) - (altura - 1));
					}

					int indice = (yInicial * largura) + x;
					for (int i = 0; i < tamanhoValido; i++, indice += largura) {
						if (valor > imagem[indice]) {
							valor = imagem[indice];
						}
					}

					temp[(y * largura) + x] = (byte)valor;
				}
			}

			// Segunda passada (horizontal)
			for (int y = 0; y < altura; y++) {
				for (int x = 0; x < largura; x++) {
					int valor = 255;

					int xInicial = x - metade;
					int tamanhoValido = tamanhoJanela;
					if (xInicial < 0) {
						tamanhoValido = tamanhoValido + xInicial;
						xInicial = 0;
					}
					if ((x + metade) > (largura - 1)) {
						tamanhoValido = tamanhoValido - ((x + metade) - (largura - 1));
					}

					int indice = (y * largura) + xInicial;
					for (int i = 0; i < tamanhoValido; i++, indice++) {
						if (valor > temp[indice]) {
							valor = temp[indice];
						}
					}

					imagem[(y * largura) + x] = (byte)valor;
				}
			}
		}
	}
}