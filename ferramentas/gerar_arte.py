"""Gera a pixel art própria do Lab 05 (A Trilha da Bolota).

O tileset fornecido (tilesetOpenGame.png) tem chão e decoração, mas não tem
personagens nem itens. Este script desenha, em pixel art no mesmo estilo:
  - esquilo (jogador): parado, correndo, pulando e caindo
  - lesma e abelha (inimigos), bolota, morango e coração (itens), brilho
  - chao.png: blocos de chão com bordas, recortados do morro do tileset
  - colinas.png e bosque.png: camadas extras do parallax

Uso: python3 ferramentas/gerar_arte.py  (grava em TrilhaDaBolota/Assets/Art/Gerados)
"""
import math
import os
from PIL import Image

AQUI = os.path.dirname(os.path.abspath(__file__))
ART = os.path.join(AQUI, "..", "TrilhaDaBolota", "Assets", "Art")
SAIDA = os.path.join(ART, "Gerados")
TILESET = os.path.join(ART, "tilesetOpenGame.png")

CONTORNO = (44, 26, 18, 255)


def hexa(cor, alfa=255):
    cor = cor.lstrip("#")
    return tuple(int(cor[i:i + 2], 16) for i in (0, 2, 4)) + (alfa,)


class Tela:
    """Desenho em camadas: cada parte recebe contorno próprio antes do preenchimento."""

    def __init__(self, largura, altura):
        self.img = Image.new("RGBA", (largura, altura), (0, 0, 0, 0))
        self.w, self.h = largura, altura

    def parte(self, pixels, cor, contorno=CONTORNO):
        pixels = {(x, y) for x, y in pixels if 0 <= x < self.w and 0 <= y < self.h}
        if contorno:
            for x, y in pixels:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    v = (x + dx, y + dy)
                    if v not in pixels and 0 <= v[0] < self.w and 0 <= v[1] < self.h:
                        self.img.putpixel(v, contorno)
        for p in pixels:
            self.img.putpixel(p, cor)

    def pinta(self, pixels, cor):
        self.parte(pixels, cor, contorno=None)

    def salva(self, nome):
        self.img.save(os.path.join(SAIDA, nome + ".png"))


def elipse(cx, cy, rx, ry):
    return {(x, y) for x in range(int(cx - rx - 1), int(cx + rx + 2))
            for y in range(int(cy - ry - 1), int(cy + ry + 2))
            if ((x + .5 - cx) / rx) ** 2 + ((y + .5 - cy) / ry) ** 2 <= 1}


def retangulo(x0, y0, x1, y1):
    return {(x, y) for x in range(x0, x1 + 1) for y in range(y0, y1 + 1)}


# ---------------------------------------------------------------- Esquilo (20x20)
LARANJA = hexa("c8682a")
LARANJA_CLARO = hexa("e89048")
CAUDA = hexa("b65a24")
CAUDA_CLARA = hexa("ef9f55")
CREME = hexa("f6dcae")
PRETO = hexa("1a1414")
BRANCO = hexa("ffffff")


def esquilo(nome, bob=0, cauda=(0, 0), pernas=((9, 0), (13, 0)), pernas_altas=False):
    t = Tela(20, 20)
    cx, cy = cauda
    # cauda felpuda atrás do corpo
    t.parte(elipse(5 + cx, 8 + cy + bob, 4.6, 6.6), CAUDA)
    t.pinta(elipse(5.5 + cx, 7 + cy + bob, 2.2, 4.2), CAUDA_CLARA)
    # pernas (atrás do corpo)
    for x, dy in pernas:
        topo = 16 + bob if not pernas_altas else 15 + bob
        t.parte(retangulo(x, topo, x + 1, min(19, 18 + dy)), LARANJA)
    # corpo e barriga
    t.parte(elipse(11.5, 13.5 + bob, 4.2, 4.0), LARANJA)
    t.pinta(elipse(13.4, 14.6 + bob, 2.0, 2.4), CREME)
    # cabeça, orelha, focinho
    t.parte({(12, 3 + bob), (12, 4 + bob), (13, 4 + bob), (12, 5 + bob)}, LARANJA_CLARO)
    t.parte(elipse(14.2, 8 + bob, 3.6, 3.3), LARANJA)
    t.pinta(elipse(16.6, 9.6 + bob, 1.8, 1.3), CREME)
    t.pinta({(15, 7 + bob)}, PRETO)
    t.pinta({(15, 6 + bob)}, BRANCO)
    t.pinta({(18, 9 + bob)}, PRETO)
    # bracinho segurando a frente do corpo
    t.parte(retangulo(15, 13 + bob, 16, 13 + bob), LARANJA_CLARO)
    t.salva(nome)


# ---------------------------------------------------------------- Lesma (16x16)
CONCHA = hexa("8f5bb5")
CONCHA_CLARA = hexa("c391e0")
CONCHA_ESCURA = hexa("5e3780")
LESMA = hexa("b9d65a")
LESMA_ESCURA = hexa("7f9f2e")


def lesma(nome, estica):
    t = Tela(16, 16)
    # corpo mole no chão; o quadro "estica" avança a cabeça
    t.parte(retangulo(1, 12, 12 + estica, 14) | elipse(13 + estica, 11.5, 2.2, 2.6), LESMA)
    t.pinta(retangulo(2, 14, 12 + estica, 14), LESMA_ESCURA)
    # antenas com olhos
    for ax in (12 + estica, 14 + estica):
        t.pinta({(ax, 7), (ax, 8)}, LESMA_ESCURA)
        t.parte({(ax, 6 - estica % 2)}, PRETO, contorno=None)
    # concha em espiral
    t.parte(elipse(6.5, 8.5, 5.2, 4.8), CONCHA)
    t.pinta(elipse(6.8, 8.2, 3.2, 2.9), CONCHA_CLARA)
    t.pinta(elipse(7.0, 8.4, 1.5, 1.4), CONCHA_ESCURA)
    t.pinta({(4, 6), (5, 5)}, BRANCO)
    t.pinta({(13 + estica, 11)}, PRETO)
    t.salva(nome)


# ---------------------------------------------------------------- Abelha (16x16)
AMARELO = hexa("f2c230")
LISTRA = hexa("2b2020")
ASA = hexa("dff3ff", 220)


def abelha(nome, asa_alta):
    t = Tela(16, 16)
    t.parte({(1, 9), (2, 9)}, LISTRA, contorno=None)  # ferrão
    if asa_alta:
        t.parte(elipse(6.5, 3.6, 2.4, 3.0) | elipse(9.8, 4.2, 2.0, 2.6), ASA, contorno=hexa("7aa6c2"))
    t.parte(elipse(8, 9.5, 5.6, 4.0), AMARELO)
    for x in (5, 8):
        t.pinta({(x, y) for y in range(5, 14)} | {(x + 1, y) for y in range(5, 14)}, LISTRA)
    t.parte(elipse(12.6, 8.6, 2.6, 2.6), LISTRA)
    t.pinta({(13, 8)}, BRANCO)
    t.pinta({(14, 4), (14, 5), (15, 3)}, LISTRA)
    if not asa_alta:
        t.parte(elipse(6.5, 6.4, 3.0, 1.8) | elipse(9.5, 6.6, 2.6, 1.6), ASA, contorno=hexa("7aa6c2"))
    t.pinta(elipse(8, 9.5, 5.6, 4.0) - elipse(8, 9.5, 4.6, 3.0) - {(x, y) for x in range(16) for y in range(0, 10)}, hexa("c99a1c"))
    t.salva(nome)


# ---------------------------------------------------------------- Itens (16x16)
CHAPEU = hexa("7a4a24")
CHAPEU_CLARO = hexa("a5683a")
NOZ = hexa("c9883e")
NOZ_CLARA = hexa("e6ad62")


def bolota(nome, largura):
    """Quadros de giro: a bolota estreita e volta (largura 1, 2/3, 1/3, 2/3)."""
    t = Tela(16, 16)
    rx = 4.6 * largura
    t.parte(elipse(8, 10, max(rx, .9), 4.6), NOZ)
    if largura > .5:
        t.pinta(elipse(8 - rx * .35, 9, max(rx * .35, .6), 2.4), NOZ_CLARA)
    t.parte(elipse(8, 6.4, max(rx + .9, 1.4), 2.6) | {(8, 2), (8, 3)}, CHAPEU)
    if largura > .5:
        t.pinta({(x, 6) for x in range(int(8 - rx), int(8 + rx) + 1) if x % 2 == 0}, CHAPEU_CLARO)
    t.pinta({(8, 14)}, CHAPEU)
    t.salva(nome)


VERMELHO = hexa("e0383a")
VERMELHO_CLARO = hexa("ff6b5e")
FOLHA = hexa("4fa83a")


def morango(nome, brilho):
    t = Tela(16, 16)
    corpo = {(x, y) for x in range(16) for y in range(5, 15)
             if abs(x + .5 - 8) <= 5.2 - max(0, y - 8) * .75}
    t.parte(corpo, VERMELHO)
    t.pinta(elipse(6, 7.5, 1.3, 1.8), VERMELHO_CLARO)
    for x, y in ((6, 10), (9, 8), (10, 11), (7, 12), (11, 7), (5, 8), (8, 11)):
        t.pinta({(x, y)}, hexa("ffe27a"))
    t.parte({(5, 4), (6, 4), (7, 3), (8, 4), (9, 3), (10, 4), (11, 4), (8, 2), (8, 1)}, FOLHA)
    if brilho:
        t.pinta({(12, 3), (13, 2), (14, 3), (13, 4), (13, 3)}, BRANCO)
    t.salva(nome)


ROSA = hexa("ff4d6d")


def coracao(nome, grande):
    t = Tela(16, 16)
    s = 1.0 if grande else .82
    pix = set()
    for x in range(16):
        for y in range(16):
            u, v = (x + .5 - 8) / (6.5 * s), -(y + .5 - 8.5) / (6.5 * s)
            if (u * u + v * v - .55) ** 3 - u * u * v * v * v <= 0:
                pix.add((x, y))
    t.parte(pix, ROSA)
    t.pinta({(5, 6), (5, 7), (6, 5)}, BRANCO)
    t.salva(nome)


def brilho(nome, raio):
    t = Tela(16, 16)
    amarelo = hexa("fff2a8")
    for i in range(8):
        a = i * math.pi / 4
        for r in (raio, raio + 1):
            x, y = round(8 + math.cos(a) * r), round(8 + math.sin(a) * r)
            t.pinta({(x, y)}, amarelo if i % 2 == 0 else BRANCO)
    if raio <= 2:
        t.pinta(retangulo(7, 7, 8, 8), BRANCO)
    t.salva(nome)


# ---------------------------------------------------------------- Chão (blocos do morro)
def gerar_chao():
    """Monta chao.png (4x5 blocos de 16px) a partir do morro do tileset.

    Linha 0: topo (canto esq., meio A, meio B, canto dir.)
    Linha 1: terra com pedras logo abaixo do topo (borda esq., meio A, meio B, borda dir.)
    Linha 2: terra escura funda (idem)
    Linha 3: plataforma fina de 1 bloco (esq., meio A, meio B, dir.)
    Linha 4: base de bloco flutuante (esq., meio A, meio B, dir.)
    """
    src = Image.open(TILESET).convert("RGBA")
    bloco = lambda c, l: src.crop((c * 16, l * 16, c * 16 + 16, l * 16 + 16))
    topo, terra, funda = [bloco(6, 8), bloco(7, 8)], [bloco(6, 9), bloco(7, 9)], [bloco(6, 10), bloco(7, 10)]
    out = Image.new("RGBA", (64, 80), (0, 0, 0, 0))
    sombra = (31, 29, 40, 255)
    grama = [topo[0].getpixel((x, y)) for x in range(16) for y in range(16) if topo[0].getpixel((x, y))[1] > 120]

    def borda(img, lado, com_grama):
        img = img.copy()
        xs = range(0, 2) if lado == "e" else range(14, 16)
        for y in range(16):
            for x in xs:
                if img.getpixel((x, y))[3] == 0:
                    continue
                externo = x in (0, 15)
                if externo and y > (5 if com_grama else -1):
                    img.putpixel((x, y), (0, 0, 0, 0))  # recorte irregular da borda
                elif y > 5 or not com_grama:
                    img.putpixel((x, y), sombra)
        if com_grama:  # a grama do topo escorre pela borda
            for y in range(5, 10):
                x = 1 if lado == "e" else 14
                if (y + (0 if lado == "e" else 1)) % 2 == 0 or y < 8:
                    img.putpixel((x, y), grama[(y * 7) % len(grama)])
        return img

    def base(img):
        img = img.copy()
        for x in range(16):
            for y in (13, 14, 15):
                if y == 15 or (y == 14 and x % 3 == 0):
                    img.putpixel((x, y), (0, 0, 0, 0))
                elif img.getpixel((x, y))[3]:
                    img.putpixel((x, y), sombra)
        return img

    linhas = [topo, terra, funda]
    for l, par in enumerate(linhas):
        out.paste(borda(par[0], "e", l == 0), (0, l * 16))
        out.paste(par[0], (16, l * 16))
        out.paste(par[1], (32, l * 16))
        out.paste(borda(par[1], "d", l == 0), (48, l * 16))
    fina = [base(topo[0]), base(topo[1])]
    out.paste(base(borda(topo[0], "e", True)), (0, 48))
    out.paste(fina[0], (16, 48))
    out.paste(fina[1], (32, 48))
    out.paste(base(borda(topo[1], "d", True)), (48, 48))
    out.paste(base(borda(funda[0], "e", False)), (0, 64))
    out.paste(base(funda[0]), (16, 64))
    out.paste(base(funda[1]), (32, 64))
    out.paste(base(borda(funda[1], "d", False)), (48, 64))
    out.save(os.path.join(SAIDA, "chao.png"))


# ---------------------------------------------------------------- Camadas do parallax
def gerar_colinas():
    """Colinas azuladas ao longe (320x256, repetível na horizontal)."""
    img = Image.new("RGBA", (320, 256), (0, 0, 0, 0))
    perto, longe = (120, 170, 150, 255), (150, 196, 196, 255)
    for x in range(320):
        a = x / 320 * 2 * math.pi
        y_longe = 150 + 18 * math.sin(a * 2 + .7) + 9 * math.sin(a * 5 + 1.3)
        y_perto = 178 + 14 * math.sin(a * 3 + 2.1) + 6 * math.sin(a * 7)
        for y in range(int(y_longe), 256):
            img.putpixel((x, y), longe)
        for y in range(int(y_perto), 256):
            img.putpixel((x, y), perto)
    img.save(os.path.join(SAIDA, "colinas.png"))


def colar_recortado(destino, imagem, x, y):
    """alpha_composite que aceita imagem parcialmente fora do destino."""
    esq, topo = max(0, -x), max(0, -y)
    dir_, base = min(imagem.width, destino.width - x), min(imagem.height, destino.height - y)
    if esq < dir_ and topo < base:
        destino.alpha_composite(imagem.crop((esq, topo, dir_, base)), (x + esq, y + topo))


def gerar_bosque():
    """Bosque em silhueta com a árvore do tileset, mais escura (320x256, repetível)."""
    src = Image.open(TILESET).convert("RGBA")
    arvore = src.crop((208, 16, 304, 128))
    tinta = (70, 128, 112)
    sombra = Image.new("RGBA", arvore.size, (0, 0, 0, 0))
    for x in range(arvore.width):
        for y in range(arvore.height):
            r, g, b, a = arvore.getpixel((x, y))
            if a:
                luz = (r + g + b) / 765
                sombra.putpixel((x, y), tuple(int(c * (.75 + luz * .5)) for c in tinta) + (255,))
    img = Image.new("RGBA", (320, 256), (0, 0, 0, 0))
    for x0, escala in ((-20, 1.0), (70, .8), (150, 1.1), (240, .9)):
        a = sombra.resize((int(96 * escala), int(112 * escala)), Image.NEAREST)
        for dx in (0, 320, -320):  # repete nas bordas para a imagem emendar sem corte
            colar_recortado(img, a, x0 + dx, 256 - 40 - a.height)
    for x in range(320):
        topo = 216 + int(4 * math.sin(x / 320 * 2 * math.pi * 6))
        for y in range(topo, 256):
            img.putpixel((x, y), tuple(int(c * .8) for c in tinta) + (255,))
    img.save(os.path.join(SAIDA, "bosque.png"))


def main():
    os.makedirs(SAIDA, exist_ok=True)
    esquilo("esquilo-parado-1")
    esquilo("esquilo-parado-2", bob=0, cauda=(0, 1))
    corrida = [((8, -1), (14, 0)), ((10, 0), (12, -1)), ((9, 0), (13, 0)), ((11, -1), (11, 0))]
    for i, pernas in enumerate(corrida):
        esquilo(f"esquilo-corre-{i + 1}", bob=-(i % 2), cauda=(i % 2 - 1, -(i % 2)), pernas=pernas)
    esquilo("esquilo-pulo", bob=-1, cauda=(-1, 2), pernas=((8, -2), (14, -2)), pernas_altas=True)
    esquilo("esquilo-queda", bob=0, cauda=(0, -2), pernas=((9, 0), (13, 0)))
    lesma("lesma-1", 0)
    lesma("lesma-2", 1)
    abelha("abelha-1", True)
    abelha("abelha-2", False)
    for i, larg in enumerate((1, .66, .3, .66)):
        bolota(f"bolota-{i + 1}", larg)
    morango("morango-1", False)
    morango("morango-2", True)
    coracao("coracao-1", True)
    coracao("coracao-2", False)
    for i, r in enumerate((2, 4, 6)):
        brilho(f"brilho-{i + 1}", r)
    gerar_chao()
    gerar_colinas()
    gerar_bosque()


if __name__ == "__main__":
    main()
