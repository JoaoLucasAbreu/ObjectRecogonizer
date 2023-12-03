# Detecção de objetos

## Algoritmo de preenchimento por inundação (flood fill)

O processo de localizar objetos em imagens pode ser realizado por centenas de métodos, que variam absurdamente no nível de complexidade, dependendo do tipo da imagem de entrada (como RGB ou escala de cinza), do tipo de informação que se deseja extrair dos objetos da cena (gênero de uma pessoa, comprimento do cabelo presente em uma foto etc.), e de como as cores estão dispostas na imagem. Em casos específicos, pode ser necessário utilizar até mesmo redes neurais ou outros tipos de técnicas de inteligência artificial. 😳

Para nossa sorte 😅, se nossa imagem de entrada estiver em escala de cinza, devidamente limiarizada (ou seja, só com valores 0 ou 255), e só necessitarmos saber a localização e dimensão dos objetos da imagem, é possível utilizar um algoritmo relativamente simples, que é o algoritmo de preenchimento por inundação ([flood fill](https://en.wikipedia.org/wiki/Flood_fill)).

Esse algoritmo é o algoritmo utilizado por programas de desenho, como Paint ou GIMP, para preencher grandes áreas da imagem que possuam a mesma cor (ou uma cor similar, o que complica um cadinho o processo, mas não vamos utilizar aqui 😅). Essa ferramenta normalmente tem o ícone de um balde de tinta.

Em uma imagem já limiarizada, podemos utilizar o flood fill para ir apagando os objetos (pixels 255) conforme percorremos a imagem. Assim, sempre que encontramos um pixel 255, "damos uma pausa" no processo de percorrer a imagem, e iniciamos um novo processo para apagar (passar o pixel para 0) os pixels vizinhos do primeiro pixel 255 encontrado. Esse processo deve ser repetido de forma recursiva, de modo a apagar os vizinhos dos vizinhos e assim por diante... 😅

Conforme apagamos os pixels, podemos coletar dados que poderão ser úteis no futuro, como a quantidade de pixels no objeto e as coordenadas limítrofes do objeto (os menores e maiores valores de X e Y do objeto).

Em linhas gerais, o algoritmo de flood fill é descrito da seguinte forma:

```
imgEntrada = Imagem de entrada, em escala de cinza, já limiarizada

largura = largura de imgEntrada
altura = altura de imgEntrada
objetosEncontrados = 0

para y de 0 até altura - 1

	para x de 0 até largura - 1

		se imgEntrada[y, x] for branco

			objetosEncontrados = objetosEncontrados + 1

			floodFill(imgEntrada, x, y)

		fim_se

	fim_para

fim_para

função floodFill(imgEntrada, x, y)

	imgEntrada[y, x] = preto

	********************************
	Agora, basta verificar todos os 4 vizinhos do pixel, e repetir o processo! Atenção!!! Devemos tomar cuidado quando x for 0, y for 0, x for largura - 1, ou y for altura - 1! Nesses casos, o teste não deve nem ocorrer, visto que não devemos acessar pixels em coordenadas negativas, ou em coordenadas que excedam os limites da imagem!
	********************************

	se imgEntrada[y - 1, x] = branco
		floodFill(imgEntrada, x, y - 1)
	fim_se

	se imgEntrada[y + 1, x] = branco
		floodFill(imgEntrada, x, y + 1)
	fim_se

	se imgEntrada[y, x - 1] = branco
		floodFill(imgEntrada, x - 1, y)
	fim_se

	se imgEntrada[y, x + 1] = branco
		floodFill(imgEntrada, x + 1, y)
	fim_se

fim_função
```

Esse código mostrado considera que um pixel possua apenas 4 vizinhos (cima, baixo, esquerda e direita). Esse comportamento é padrão em diversas ferramentas e implementações desse algoritmo, mas também é possível considerar que um pixel tenha 8 vizinhos (incluindo também as diagonais). Tudo depende do desejo de uso e da disposição esperada dos objetos na tela. A diferença das duas implementações pode ser percebida na imagem `Diferentes Objetos.png` do Canvas.

### Prática

- Baixe a imagem `Diferentes Objetos.png`, que é uma imagem em escala de cinza, e aplique nela o algoritmo apresentado anteriormente (com 4 vizinhos) para contar a quantidade de objetos presentes da imagem.

- Baixe a imagem `Diferentes Objetos.png`, que é uma imagem em escala de cinza, e aplique nela o algoritmo apresentado anteriormente (com 8 vizinhos) para contar a quantidade de objetos presentes da imagem.

- **(Intermediário)** Baixe a imagem `Diferentes Objetos.png`, que é uma imagem em escala de cinza, e aplique nela o algoritmo apresentado anteriormente (com 4 vizinhos) para contar a quantidade de objetos presentes da imagem. Conforme o algoritmo de flood fill for sendo aplicado, conte a quantidade de pixels pintados e calcule os menores/maiores valores das coordenadas X e Y dos objetos encontrados, exibindo na tela um resumo para cada objeto encontrado, como exibido abaixo:
```
Objeto 0 - Pixels pintados: 460 / Menor X: 87 / Menor Y: 14 / Maior X: 114 / Maior Y: 41
Objeto 1 - Pixels pintados: 43 / Menor X: 39 / Menor Y: 18 / Maior X: 55 / Maior Y: 31
...
```

- **(Avançado / Opcional)** Baixe a imagem `Diferentes Objetos RGB.png`, que é uma imagem RGB, e converta ela para escala de cinza utilizando uma média aritmética simples. Em seguida, limiarize a imagem em escala de cinza utilizando um limiar global, calculado pela média aritmética dos pixels em escala de cinza. Por fim, aplique na imagem em escala de cinza o algoritmo apresentado anteriormente (com 4 vizinhos) para contar a quantidade de objetos presentes da imagem. Conforme o algoritmo de flood fill for sendo aplicado, conte a quantidade de pixels pintados e calcule os menores/maiores valores das coordenadas X e Y dos objetos encontrados, exibindo na tela um resumo para cada objeto encontrado, como exibido abaixo:
```
Objeto 0 - Pixels pintados: 460 / Menor X: 87 / Menor Y: 14 / Maior X: 114 / Maior Y: 41
Objeto 1 - Pixels pintados: 43 / Menor X: 39 / Menor Y: 18 / Maior X: 55 / Maior Y: 31
...
```

--------------------------------------------------------------------------------

# Mais referências

Flood fill
https://en.wikipedia.org/wiki/Flood_fill

SKBitmap.Decode Method
https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.decode

SKBitmap.Encode Method
https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode

FileStream Class
https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream

using statement - ensure the correct use of disposable objects
https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/using

SKBitmap Class
https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap

SKImageInfo Struct
https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimageinfo
