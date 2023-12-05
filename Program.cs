using SkiaSharp;

namespace Projeto {

	class Bola {
		public int x { get; set; }
    	public int y { get; set; }
	}

	class Program {
		static void Main(string[] args) {
			
			Console.WriteLine("Número da cena: ");
			string input = Console.ReadLine();
			int.TryParse(input, out int cena);

			Console.WriteLine("Número de quadros: ");
			input = Console.ReadLine();
			int.TryParse(input, out int quadros);

			Bola ultimoLocalBola = null;

			List<double> velocidades = new List<double>();
			int? primeiroQuadroMov = null;

			for (int q = 0; q < quadros; q++)
			{
				using (
					SKBitmap bitmapEntrada = SKBitmap.Decode($"img\\raw\\Cena{cena}_{q}.png"),
					bitmapSaida = new SKBitmap(new SKImageInfo(bitmapEntrada.Width, bitmapEntrada.Height, SKColorType.Gray8))) {

					unsafe {
						byte* entrada = (byte*)bitmapEntrada.GetPixels();
						byte* saida = (byte*)bitmapSaida.GetPixels();

						long pixelsTotais = bitmapEntrada.Width * bitmapEntrada.Height;

						// Limiarização
						for (int e = 0, s = 0; s < pixelsTotais; e += 4, s++) {
							
							if (entrada[e + 1] > entrada[e] && entrada[e + 1] > entrada[e + 2])
								saida[s] = 0;
							else 
								saida[s] = 255;
						}

						// Erosão
						Erodir(saida, bitmapSaida.Width, bitmapSaida.Height, 7, entrada);

						var formas = Forma.DetectarFormas(saida, bitmapSaida.Width, bitmapSaida.Height, true);

						bool temBola = false;
						foreach (var forma in formas) 
						{
							//Console.WriteLine($"Quadro_{1}: Altura={forma.Altura} Largura={forma.Largura} Area={forma.Largura * forma.Altura}");
							if (forma.Largura * forma.Altura > 5500 && forma.Largura * forma.Altura < 8500 && forma.CentroX > bitmapSaida.Width/2) 
							{
								temBola = true;
								if (ultimoLocalBola != null)
								{
									if ((ultimoLocalBola.x != forma.CentroX || ultimoLocalBola.y != forma.CentroY) && primeiroQuadroMov == null)
										primeiroQuadroMov = q;

									var cmPorPX =(double)30 / forma.Largura;
									var deltax = (double)ultimoLocalBola.x - forma.CentroX;
									var deltay = (double)ultimoLocalBola.y - forma.CentroY;
									var distancia = Math.Sqrt(deltax * deltax + deltay * deltay) * cmPorPX;
									var velocidade = distancia/0.02;

									Console.WriteLine($"{q - 1} ==> {q}: {velocidade:0.00}cm/s");

									if (velocidade != 0)
										velocidades.Add(velocidade);

									ultimoLocalBola.x = forma.CentroX;
									ultimoLocalBola.y = forma.CentroY;
								} else {
									ultimoLocalBola = new Bola {
										x = forma.CentroX,
										y = forma.CentroY
									};
								}
							}	
						}

						if (!temBola)
							throw new Exception("Não tem bola");
					}

					using (FileStream stream = new FileStream($"img\\limiar\\Cena{cena}_{q}.png", FileMode.OpenOrCreate, FileAccess.Write)) {
						bitmapSaida.Encode(stream, SKEncodedImageFormat.Png, 100);
					}
				}
			}

			if (primeiroQuadroMov != null) 
				Console.WriteLine($"Primeiro quadro onde a bola se moveu: {primeiroQuadroMov}");
			else 
				Console.WriteLine("A bola não se moveu");

			double vm = 0;
			foreach (var v in velocidades)
			{
				vm = vm + v;
			}

			Console.WriteLine($"Velocidade média da bola: {vm / velocidades.Count():0.00}cm/s");
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