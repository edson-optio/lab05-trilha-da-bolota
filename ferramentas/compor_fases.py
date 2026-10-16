"""Compõe os mapas de texto das duas fases (Assets/Fases/fase1.txt e fase2.txt).

Altura e posição de cada trecho de chão, plataforma, item e inimigo ficam aqui,
em coordenadas de bloco (x da esquerda para a direita, y de baixo para cima).
Regras usadas no desenho: degraus de no máximo 2 blocos e buracos de até 4.
"""
import os
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "TrilhaDaBolota", "Assets", "Fases") + os.sep
class M:
    def __init__(s,w,h): s.w,s.h=w,h; s.g=[['.']*w for _ in range(h)]
    def put(s,x,y,c):
        assert 0<=x<s.w and 0<=y<s.h,(x,y,c); s.g[s.h-1-y][x]=c
    def ground(s,x0,x1,top):
        for x in range(x0,x1+1):
            for y in range(top): s.put(x,y,'#')
    def plat(s,x0,x1,y):
        for x in range(x0,x1+1): s.put(x,y,'#')
    def save(s,name,header):
        txt="\n".join("".join(r) for r in s.g)
        open(OUT+name,"w").write(header+txt+"\n")

hdr="""# Mapa da fase (cada caractere = 1 bloco de 16 px; a última linha é o fundo do mapa)
# #  chão com colisão (o construtor escolhe topo, borda e terra)
# P  início do esquilo        S  placa (ponto de retorno)
# a  bolota (obrigatória)     s  morango (+50)     h  coração (+1 vida)
# L  lesma (anda no chão)     B  abelha (voa)
# T  árvore  F  cerca  W  girassóis  R  pedra  G  poste  C  baú  H  escada  (decoração)
"""
m=M(100,14)
m.ground(0,15,3); m.put(2,3,'P'); m.put(4,3,'T'); m.put(12,3,'W'); m.put(9,4,'a')
m.ground(16,29,5); m.put(18,6,'a'); m.put(25,5,'L'); m.put(27,6,'s'); m.put(21,5,'R')
m.ground(33,48,3); m.plat(37,41,4); m.put(39,6,'a'); m.put(45,6,'B'); m.put(34,3,'S'); m.put(42,3,'F')
m.ground(49,55,5); m.put(52,6,'a'); m.put(50,5,'W')
m.ground(56,70,3); m.put(62,3,'L'); m.plat(58,61,5); m.plat(63,66,7); m.put(64,8,'a'); m.put(59,6,'h'); m.put(69,4,'a'); m.put(67,3,'G')
m.ground(74,99,3); m.put(75,3,'S'); m.put(80,6,'B'); m.put(84,4,'s'); m.plat(86,89,5); m.put(87,6,'a'); m.put(91,3,'L'); m.put(94,3,'G'); m.put(97,4,'a'); m.put(78,3,'R')
m.save("fase1.txt",hdr)
m=M(120,16)
m.ground(0,12,3); m.put(2,3,'P'); m.put(4,3,'T'); m.put(10,3,'W')
m.ground(16,22,4); m.put(19,5,'a'); m.put(20,4,'L')
m.plat(24,25,4); m.put(25,6,'a')
m.ground(27,35,3); m.put(31,6,'B'); m.put(33,4,'a'); m.put(28,3,'S')
m.plat(37,39,4); m.plat(41,43,6); m.plat(45,47,8); m.put(46,10,'a'); m.put(42,8,'h')
m.ground(49,60,5); m.put(53,5,'L'); m.put(58,5,'L'); m.put(56,7,'s'); m.put(51,6,'a'); m.put(50,5,'R')
m.ground(65,75,3); m.put(68,6,'B'); m.put(73,6,'B'); m.put(70,4,'a'); m.put(66,3,'S')
m.plat(77,79,4); m.plat(82,84,5); m.put(83,7,'a'); m.put(86,8,'B'); m.plat(87,89,4)
m.ground(91,104,3); m.put(95,3,'L'); m.put(93,4,'s'); m.put(99,4,'a'); m.plat(100,103,5); m.put(102,7,'a'); m.put(92,3,'S')
m.ground(108,119,3); m.put(112,4,'a'); m.put(109,3,'G'); m.put(111,3,'F'); m.put(117,3,'C')
m.save("fase2.txt",hdr)
