using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;
using Particula;
using System.Xml;


// Vetores principais / Variáveis naturais
Vector2 gravidade = new Vector2(0, 980);
int screenHeight = 450;
int screenWidth = 800;
float tamanhoCelula = 40f;

// Lista de particulas
List<Particula.Particula> particulas = new List<Particula.Particula>();


// Início do programa
Raylib.InitWindow(screenWidth, screenHeight, "Open Window");
Raylib.SetTargetFPS(60);

while (!Raylib.WindowShouldClose())
{
    // Lógica do motor
    float dt = Raylib.GetFrameTime();
    Dictionary<(int, int), List<Particula.Particula>> grid = new();

    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        particulas.Add(new Particula.Particula { Posicao = mousePos, Velocidade = Vector2.Zero, Raio = 10, Cor = Color.White});
    }

    foreach (var p in particulas)
    {
        int cx = (int)(p.Posicao.X / tamanhoCelula);
        int cy = (int)(p.Posicao.Y / tamanhoCelula);
        var chave = (cx, cy);

        if (!grid.ContainsKey(chave))
        {
            grid[chave] = new List<Particula.Particula>();
        }
        grid[chave].Add(p);
    }

    // Offsets das 9 células (a própria + 8 vizinhas), só "pra frente"
    // pra não testar o mesmo par de células duas vezes
    (int, int)[] offsets = new (int, int)[]
    {
        (0, 0), (1, 0), (-1, 1), (0, 1), (1, 1)
    };

    foreach (var chaveOrigem in grid.Keys)
    {
        List<Particula.Particula> celulaOrigem = grid[chaveOrigem];

        foreach (var offset in offsets)
        {
            var chaveVizinha = (chaveOrigem.Item1 + offset.Item1, chaveOrigem.Item2 + offset.Item2);

            if (!grid.ContainsKey(chaveVizinha)) continue;

            List<Particula.Particula> celulaVizinha = grid[chaveVizinha];

            bool mesmaCelula = chaveOrigem == chaveVizinha;

            for (int i = 0; i < celulaOrigem.Count; i++)
            {
                int jInicio = mesmaCelula ? i + 1 : 0;

                for (int j = jInicio; j < celulaVizinha.Count; j++)
                {
                    Particula.Particula a = celulaOrigem[i];
                    Particula.Particula b = celulaVizinha[j];

                    float distancia = Vector2.Distance(a.Posicao, b.Posicao);
                    float somaRaios = a.Raio + b.Raio;

                    if (distancia < somaRaios)
                    {
                        Vector2 direcaoColisao;
                        float sobreposicao = somaRaios - distancia;

                        if (distancia > 0.001f)
                        {
                            direcaoColisao = Vector2.Normalize(a.Posicao - b.Posicao);
                        }
                        else
                        {
                            direcaoColisao = new Vector2(1, 0);
                        }

                        Vector2 velRelativa = a.Velocidade - b.Velocidade;
                        float velAoLongoNormal = Vector2.Dot(velRelativa, direcaoColisao);

                        if (velAoLongoNormal < 0)
                        {
                            float restituicao = 0.6f;
                            float impulso = -(1 + restituicao) * velAoLongoNormal / 2;

                            a.Velocidade += direcaoColisao * impulso;
                            b.Velocidade -= direcaoColisao * impulso;
                        }

                        a.Posicao += direcaoColisao * (sobreposicao / 2);
                        b.Posicao -= direcaoColisao * (sobreposicao / 2);
                    }
                }
            }
        }
    }

    foreach (Particula.Particula p in particulas)
    {
        p.Velocidade += gravidade * dt;
        p.Posicao += p.Velocidade * dt;

        if(p.Posicao.Y >= screenHeight - p.Raio)
        {
            p.Posicao.Y = screenHeight - p.Raio;
            p.Velocidade.Y *= -0.6f;
        }
        if (p.Posicao.X <= p.Raio)
        {
            p.Posicao.X = p.Raio;
            p.Velocidade.X *= -0.6f;
        }
        if (p.Posicao.X >= screenWidth - p.Raio)
        {
            p.Posicao.X = screenWidth - p.Raio;
            p.Velocidade.X *= -0.6f;
        }
        if (p.Posicao.Y <= p.Raio)
        {
            p.Posicao.Y = p.Raio;
            p.Velocidade.Y *= -0.6f;
        }
    }

    // Lógica desenhista
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);
    Raylib.DrawFPS(10, 10);

    foreach (Particula.Particula p in particulas)
    {
        Raylib.DrawCircle((int)p.Posicao.X, (int)p.Posicao.Y, p.Raio, p.Cor);
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();