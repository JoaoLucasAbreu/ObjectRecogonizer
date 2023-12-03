using System.Text;

namespace Projeto {
	public class Forma {
		private class ElementoPilha {
			public readonly int X, Y, I;

			public ElementoPilha(int x, int y, int i) {
				X = x;
				Y = y;
				I = i;
			}
		}

		private static unsafe void ConsumirForma(byte* imagem, int largura, int altura, Forma forma, Stack<ElementoPilha> pilha, bool considerar8Vizinhos, Forma[]? mapa = null) {
			while (pilha.Count != 0) {
				int x, y, i, oldI;
				ElementoPilha e = pilha.Pop();

				x = e.X;
				y = e.Y;

				// Verifica acima
				i = e.I - largura;
				if (y > 0 && imagem[i] == 255) {
					forma.AdicionarPixel(x, y - 1);
					if (mapa != null) {
						mapa[i] = forma;
					}
					imagem[i] = 254;
					pilha.Push(new ElementoPilha(x, y - 1, i));
				}

				// Verifica abaixo
				i = e.I + largura;
				if (y < (altura - 1) && imagem[i] == 255) {
					forma.AdicionarPixel(x, y + 1);
					if (mapa != null) {
						mapa[i] = forma;
					}
					imagem[i] = 254;
					pilha.Push(new ElementoPilha(x, y + 1, i));
				}

				// Vai tudo até a esquerda, verificando acima e abaixo
				x--;
				i = e.I - 1;
				while (x > 0 && imagem[i] == 255) {
					forma.AdicionarPixel(x, y);
					if (mapa != null) {
						mapa[i] = forma;
					}
					imagem[i] = 254;

					oldI = i;

					// Verifica acima
					i = oldI - largura;
					if (y > 0 && imagem[i] == 255) {
						forma.AdicionarPixel(x, y - 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y - 1, i));
					}

					// Verifica abaixo
					i = oldI + largura;
					if (y < (altura - 1) && imagem[i] == 255) {
						forma.AdicionarPixel(x, y + 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y + 1, i));
					}

					i = oldI - 1;
					x--;
				}

				// Última verificação (porque utilizamos os 8 vizinhos): as diagonais
				if (considerar8Vizinhos && x >= 0) {
					oldI = i;

					// Verifica acima
					i = oldI - largura;
					if (y > 0 && imagem[i] == 255) {
						forma.AdicionarPixel(x, y - 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y - 1, i));
					}

					// Verifica abaixo
					i = oldI + largura;
					if (y < (altura - 1) && imagem[i] == 255) {
						forma.AdicionarPixel(x, y + 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y + 1, i));
					}
				}

				// Agora, vai tudo até a direita, verificando acima e abaixo
				x = e.X + 1;
				i = e.I + 1;
				while (x < largura && imagem[i] == 255) {
					forma.AdicionarPixel(x, y);
					if (mapa != null) {
						mapa[i] = forma;
					}
					imagem[i] = 254;

					oldI = i;

					// Verifica acima
					i = oldI - largura;
					if (y > 0 && imagem[i] == 255) {
						forma.AdicionarPixel(x, y - 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y - 1, i));
					}

					// Verifica abaixo
					i = oldI + largura;
					if (y < (altura - 1) && imagem[i] == 255) {
						forma.AdicionarPixel(x, y + 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y + 1, i));
					}

					i = oldI + 1;
					x++;
				}

				// Última verificação (porque utilizamos os 8 vizinhos): as diagonais
				if (considerar8Vizinhos && x < largura) {
					oldI = i;

					// Verifica acima
					i = oldI - largura;
					if (y > 0 && imagem[i] == 255) {
						forma.AdicionarPixel(x, y - 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y - 1, i));
					}

					// Verifica abaixo
					i = oldI + largura;
					if (y < (altura - 1) && imagem[i] == 255) {
						forma.AdicionarPixel(x, y + 1);
						if (mapa != null) {
							mapa[i] = forma;
						}
						imagem[i] = 254;
						pilha.Push(new ElementoPilha(x, y + 1, i));
					}
				}
			}
		}

		public static unsafe List<Forma> CriarMapaDeFormas(byte* imagem, int largura, int altura, bool considerar8Vizinhos, out Forma[] mapaDeFormas) {
			List<Forma> formasIndividuais = new List<Forma>();
			Forma[] mapa = new Forma[largura * altura];
			Stack<ElementoPilha> pilha = new Stack<ElementoPilha>(2048);

			int i = 0;
			for (int y = 0; y < altura; y++) {
				for (int x = 0; x < largura; x++, i++) {
					if (imagem[i] == 255) {
						Forma forma = new Forma(x, y);
						pilha.Push(new ElementoPilha(x, y, i));
						imagem[i] = 254;
						mapa[i] = forma;
						ConsumirForma(imagem, largura, altura, forma, pilha, considerar8Vizinhos, mapa);
						forma.AtualizarCentro();
						formasIndividuais.Add(forma);
					}
				}
			}

			mapaDeFormas = mapa;

			return formasIndividuais;
		}

		public static unsafe List<Forma> DetectarFormas(byte* imagem, int largura, int altura, bool considerar8Vizinhos) {
			List<Forma> formasIndividuais = new List<Forma>();
			Stack<ElementoPilha> pilha = new Stack<ElementoPilha>(2048);

			int i = 0;
			for (int y = 0; y < altura; y++) {
				for (int x = 0; x < largura; x++, i++) {
					if (imagem[i] == 255) {
						Forma forma = new Forma(x, y);
						pilha.Push(new ElementoPilha(x, y, i));
						imagem[i] = 254;
						ConsumirForma(imagem, largura, altura, forma, pilha, considerar8Vizinhos);
						forma.AtualizarCentro();
						formasIndividuais.Add(forma);
					}
				}
			}

			return formasIndividuais;
		}

		public int Area, X0, Y0, X1, Y1, CentroX, CentroY;

		public int Largura {
			get {
				return X1 - X0 + 1;
			}
		}

		public int Altura {
			get {
				return Y1 - Y0 + 1;
			}
		}

		public Forma(int x, int y) {
			Area = 1;
			X0 = x;
			Y0 = y;
			X1 = x;
			Y1 = y;
			CentroX = x;
			CentroY = y;
		}

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Area: ");
			stringBuilder.Append(Area);
			stringBuilder.Append(" / X0: ");
			stringBuilder.Append(X0);
			stringBuilder.Append(" / Y0: ");
			stringBuilder.Append(Y0);
			stringBuilder.Append(" / X1: ");
			stringBuilder.Append(X1);
			stringBuilder.Append(" / Y1: ");
			stringBuilder.Append(Y1);
			stringBuilder.Append(" / CentroX: ");
			stringBuilder.Append(CentroX);
			stringBuilder.Append(" / CentroY: ");
			stringBuilder.Append(CentroY);
			return stringBuilder.ToString();
		}

		public void AdicionarPixel(int x, int y) {
			Area++;
			if (X0 > x) {
				X0 = x;
			}
			if (Y0 > y) {
				Y0 = y;
			}
			if (X1 < x) {
				X1 = x;
			}
			if (Y1 < y) {
				Y1 = y;
			}
		}

		public void AtualizarCentro() {
			CentroX = (X1 + X0) / 2;
			CentroY = (Y1 + Y0) / 2;
		}

		public bool ContemPonto(int x, int y) {
			return (x >= X0 && x <= X1 && y >= Y0 && y <= Y1);
		}

		public bool FazInterseccao(int x0, int y0, int x1, int y1) {
			return (x0 <= X1 && x1 >= X0 && y0 <= Y1 && y1 >= Y0);
		}
	}
}
