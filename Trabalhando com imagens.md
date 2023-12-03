# Trabalhando com imagens

## Projeto inicial

Para o primeiro exemplo de como trabalhar com imagens, vamos utilizar o projeto de exemplo.

Depois de extrair o arquivo `zip` para uma pasta, e abrir essa pasta no VS Code, execute `dotnet build` no terminal, para garantir que todas as dependências estejam instaladas.

Ao longo do curso, utilizaremos a biblioteca `SkiaSharp` para carregar e gravar imagens. Isso porque as formas tradicionais de acessar imagens em .NET funcionam bem em ambiente Windows, mas tem problemas/pode não funcionar em Mac/Linux. 😶

O projeto de exemplo já tem as bibliotecas `SkiaSharp` e `SkiaSharp.NativeAssets.Linux` instaladas. Mas, bastaria executar os comandos abaixo para instalar as dependências em um projeto novo:

```
dotnet add package SkiaSharp
dotnet add package SkiaSharp.NativeAssets.Linux
```

> Substitua as strings `"Caminho da imagem de entrada"` e `"Caminho da imagem de saída"` por dois caminhos válidos de onde ler/escrever as imagens. Por exemplo `"C:\\Users\\Usuario\\Destop\\teste.png"`. **Atenção para o fato de que `\` é escrito como `\\` em C/C++/C#/Java/JavaScript!**

Código do método `main()` do projeto de exemplo:

```csharp
using (SKBitmap bitmap = SKBitmap.Decode("Caminho da imagem de entrada")) {
	Console.WriteLine(bitmap.ColorType);

	IntPtr pixels = bitmap.GetPixels();

	unsafe {
		byte* ptr = (byte*)pixels;

		ptr[0] = 0; // B
		ptr[1] = 255; // G
		ptr[2] = 255; // R
		ptr[3] = 255; // A
	}

	using (FileStream stream = new FileStream("Caminho da imagem de saída", FileMode.OpenOrCreate, FileAccess.Write)) {
		bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
	}
}
```

A instrução `using` é a forma do C# para destruir/liberar automaticamente um objeto que consome muitos recursos, como uma imagem, uma conexão web, uma conexão com banco de dados, um arquivo no disco etc.

Para mais informações sobre o `using` e as classes `SKBitmap` e `FileStream`, dê uma olhada nos documentos na seção "Mais referências".

Esse projeto abre um arquivo qualquer, exibe na tela o tipo das cores da imagem (que veremos a seguir) e altera o primeiro pixel da imagem por amarelo.

Faça o teste! Crie uma imagem qualquer (cujo primeiro pixel não seja amarelo 😅), troque as strings dos caminhos, execute o projeto e veja se uma nova imagem foi criada igual à original, mas com o primeiro pixel trocado. 🥰🙏

## Ordem das componentes (ColorType)

Apesar de ser comum utilizar o conceito de RGB para se referir às cores em ambientes de computação, como estamos trabalhando com bytes crus, esbarramos em uma questão curiosa!!! Dependendo da biblioteca utilizada e do formato do arquivo, as componentes R, G e B podem estar dispostas em diferentes ordens na memória.

Por exemplo, o amarelo é R 255, G 255 e B 0.

Se o `SkiaSharp` indicasse que o `ColorType` é `Bgra8888`, esse pixel ficaria armazenado dessa forma na memória (assumindo que a imagem esteja alocada no endereço `1000`).

```
Endereço | Valor decimal (byte)
1000     | 0   B
1001     | 255 G
1002     | 255 R
1003     | 255 A
...
```

> A componente A representa o alpha (transparência) do pixel, onde 0 é completamente transparente e 255 é completamente opaco. Ao longo do curso utilizaremos apenas imagens totalmente opacas, então A sempre será 255. Mesmo porque, trabalhar com pixels semi-transparentes, como uma opacidade de 50%, requer mais conhecimentos sobre os conceitos de "alpha pré-multiplicado e composição" (["premultiplied alpha and alpha composing"](https://microsoft.github.io/Win2D/WinUI3/html/PremultipliedAlpha.htm)) e "modos de mesclagem" ([blend modes](https://en.wikipedia.org/wiki/Blend_modes)). 😱😅

Caso o `SkiaSharp` indicasse que o `ColorType` é `Rgba8888`, esse pixel ficaria armazenado dessa forma na memória (assumindo que a imagem esteja alocada no endereço `1000`).

```
Endereço | Valor decimal (byte)
1000     | 255 R
1001     | 255 G
1002     | 0   B
1003     | 255 A
...
```

Caso seja necessário, é possível forçar a ordem das componentes durante a decodificação, utilizado a classe `SKCodec` e a estrutura `SKImageInfo`, como no código abaixo, que força a imagem a ser decodificada na ordem `Bgra8888`:

```csharp
using (SKCodec codec = SKCodec.Create("Caminho da imagem de entrada")) {
	using (SKBitmap bitmap = SKBitmap.Decode(codec, new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888))) {
		// Mesmo código de antes
	}
}
```

## Endereçamento de pixels

Apesar dos arrays serem unidimensionais, é possível utilizá-los para representar estruturas 2D, 3D e até com mais dimensões! 😱🥰

No caso de imagens bidimensionais, cada pixel tem um índice único `i`, sequencial, de modo que o pixel da primeira coluna da primeira linha da imagem (canto superior esquerdo) tem índice 0. O segundo pixel da imagem (segunda coluna, primeira linha) tem índice 1, e assim por diante. Por exemplo, uma imagem com 3 pixels de largura e 5 pixels de altura teria a seguinte distribuição de índices:

```
y / x   0    1    2
      ----------------
0     | 0  | 1  | 2  |
      ----------------
1     | 3  | 4  | 5  |
      ----------------
2     | 6  | 7  | 8  |
      ----------------
3     | 9  | 10 | 11 |
      ----------------
4     | 12 | 13 | 14 |
      ----------------
```

Ou seja, o pixel na coordenada x = 1 e y = 3, tem índice 10.

De modo geral, algumas fórmulas para ajudar... 😅

### De coordenadas x e y para índice do array (i)

```csharp
i = (y * largura) + x;
```

### De índice do array (i) para coordenadas x e y

```csharp
x = i % largura;
y = i / largura; // Divisão inteira, sem arredondamento!
```

Se cada pixel da imagem ocupasse exatamente 1 byte, a fórmula terminaria por aí. Porém, como nossas imagens possuem 4 bytes por pixel, as fórmulas ficam um pouquinho diferentes: 😅

### De coordenadas x e y para índice do array com 4 bytes por pixel (i)

```csharp
i = ((y * largura) + x) * 4;
```

### De índice do array com 4 bytes por pixel (i) para coordenadas x e y

```csharp
x = (i / 4) % largura;
y = (i / 4) / largura; // Divisão inteira, sem arredondamento!
```

## Imagens vazias

O método `SKBitmap.Decode()` serve para carregar uma imagem existente. Já o construtor da classe `SKBitmap`, junto com a estrutura `SKImageInfo`, cria uma imagem inicialmente vazia para ser preenchida por nosso código, como no exemplo abaixo:

```csharp
using (SKBitmap bitmap = new SKBitmap(new SKImageInfo(LarguraDesejada, AlturaDesejada, SKColorType.Bgra8888))) {
	// Código para trabalhar com a imagem
}
```

## Prática

- Crie uma imagem vazia com 256 pixels de largura e 1 pixel de algura, preencha os pixels com um gradiente horizontal (degradê) de preto para vermelho e grave a imagem em um arquivo PNG.

- Crie uma imagem vazia com 256 pixels de largura e 10 pixels de algura, preencha os pixels com um gradiente horizontal (degradê) de preto para vermelho e grave a imagem em um arquivo PNG.

- Crie uma imagem vazia com 50 pixels de largura e 10 pixels de algura, preencha os pixels com um gradiente horizontal (degradê) de preto para vermelho e grave a imagem em um arquivo PNG.

--------------------------------------------------------------------------------

# Mais referências

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
