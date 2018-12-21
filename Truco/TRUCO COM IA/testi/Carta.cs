using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Resources;

namespace testi
{
    class Carta // Classe de construção da carta
    {
        public char Naipe { get; set; } // Define o naipe da carta
        public int Valor { get; set; } // Define o valor da carta
        public bool Mao { get; set; } // Gerencia se a carta está na mão ou se foi jogada
        public bool Mesa { get; set; } // Gerencia se a carta está na mesa 
        static private Image Imagem; // Responsável pela a imagem da carta no jogo
        static public int Vira; // Campo que gerencia o valor da vira atual
        static IDictionary<string, Image> Deck = new Dictionary<string, Image>(); // Baralho com as cartas
        

        public Carta(char naipe, int valor)
        { // Construtor da carta

            Naipe = naipe;
            Valor = valor;
            Mao = true;
            Mesa = false;
            Valor = ChecarManilha(Valor, Naipe);
        }

        private int ChecarManilha(int v, char n)
        { // Checa se a carta construída é uma manilha (de acordo com a Vira)

            if (Vira == (v - 1) || (v == 4 && Vira == 13))
            {

                switch (n)
                {

                    case 'O': v = 14; break;
                    case 'E': v = 15; break;
                    case 'C': v = 16; break;
                    case 'P': v = 17; break;

                }

            }
            else
            {

                var value = v;
                v = value;

            }

            return v;

        }

        public static string FormatarManilha(int man) // Formata a manilha na exibição do jogo, se for carta de figura
        {

            var novaManilha = man == 8 ? "Q" : (man == 9 ? "J" : (man == 10 ? "K" : (man == 11 ? "A" : (man == 12 ? "2" : "3"))));
            return novaManilha;

        }



        public Image AdicionarImagem(char naipe, int valor)
        { //Exibe a imagem da carta correspondente ao naipe e ao valor, dentro da Picturebox

            string key = naipe.ToString() + valor.ToString();

            Imagem = Deck[key];

            return Imagem;

        }

        public static void IniciarRecursos() // Armazena os recursos gráficos das cartas do jogo no baralho
        {

            ResourceSet res = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            foreach (DictionaryEntry de in res)
            {
                var nome = de.Key.ToString();
                var carta = de.Value;

                if (de.Value is Bitmap)
                {

                    if (nome != "verso")
                    {
                        Deck.Add(nome, (Image)carta);

                    }
                }

            }
        }

    }
}
