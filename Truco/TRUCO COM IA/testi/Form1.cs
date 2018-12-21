using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;

namespace testi
{
    public partial class Form1 : Form
    {
        // Variáveis globais da classe principal:

        #region
        const byte X = 251, Y = 225, Y1 = 155;
        byte pontos1 = 0, pontos2, rodadas = 0, p1 = 0, p2 = 0, empate = 0, jogadas = 0, valorMao = 1;
        int indice = 0, indice2 = 0;
        bool vez = true, IAJogada, cartaVirada = false, vencedor = false, acabouVaza = false, pedidoTruco = false, jaPediu = false, virar = false, todasRepetidas = false;
        float chanceTruco = 0.0f;
        string[] pedidosTruco = { "É TRUCOOOO!!", "Tá trucado, quero ver agora!!", "TRUCO!! CORRE!!", "Truco ladrão!", "Quac quac, truco!", "É TRUCOOO AGORA EU QUERO VER!!", "Truco. Aceita ou corre!!", "Corre que é truco!", "Truquei, vem ver se foi blefe haha", "Trucando", "TRUCO!! Vem se for bife" };
        string[] pedido6 = { "6 na mesa!!", "6!! Agora dobrou!", "6!!!", "Meia vara!" };
        string[] pedido9 = { "É NOVEEEE!!", "9 LADRÃO", "9 ACEITA OU CORRE!" };
        string[] pedido12 = { "12 é tudo ou nada!", "Vou em 12, vai correr?", "12!!" };
        private PictureBox[] pics = new PictureBox[6];
        List<Carta> cartas = new List<Carta>();
        List<string> repetidas = new List<string>();
        List<Image> cartasImagem = new List<Image>();
        List<string> vencedorOrdem = new List<string>();
        Random rdn = new Random();
        char[] Naipes = { 'O', 'P', 'C', 'E' };
        int[] valorJogado = new int[2];
        int[] indices = new int[3];
        Label[] labels = new Label[3];
        SoundPlayer[] audios = {new SoundPlayer(Properties.Resources.N1), new SoundPlayer(Properties.Resources.N21)};
        #endregion

        // Inicia os componentes:

        public Form1()
        {
            InitializeComponent();
            Carta.IniciarRecursos();
            pics[0] = pictureBox1;
            pics[1] = pictureBox2;
            pics[2] = pictureBox3;
            pics[3] = pictureBox4;
            pics[4] = pictureBox5;
            pics[5] = pictureBox6;

            labels[0] = lbl1;
            labels[1] = lbl2;
            labels[2] = lbl3;


            SortearVira();
            SortearVez();
        }

        // Sorteia o iniciante da vez:

        private void SortearVez()
        {

            var sorteio = rdn.Next(0, 2);

            vez = sorteio == 0;

            if (!vez)
            {
                AnalisarTruco();
                JogadaIARodada1();
            }

        }

        // Sorteia uma carta do baralho para ser a VIRA e define a manilha:

        private void SortearVira()
        {

            var newNaipe = Naipes[rdn.Next(0, Naipes.Length)];
            var sorteio = rdn.Next(4, 14);
            Carta vira = new Carta(newNaipe, sorteio);
            pbVira.Image = vira.AdicionarImagem(newNaipe, sorteio);
            repetidas.Add(newNaipe.ToString() + sorteio.ToString());
            Carta.Vira = sorteio;

            var Manilha = Carta.Vira != 13 ? Carta.Vira + 1 : 4;

            if (Manilha >= 8)
            {
                var nM = Carta.FormatarManilha(Manilha);
                lblManilha.Text = nM;
            }
            else
            {
                lblManilha.Text = Manilha.ToString();
            }

            Random();

        }

        // Sorteia do baralho 6 cartas diferentes (3 para o player e 3 para a I.A):

        private void Random()
        {
            {
                for (int i = 0; i <= 5; i++)
                {
                    bool achou = false;

                    while (!achou)
                    {

                        var newNaipe = Naipes[rdn.Next(0, Naipes.Length)];
                        var sorteio = rdn.Next(4, 14);

                        if (!repetidas.Contains(newNaipe.ToString() + sorteio.ToString()))
                        {
                            cartas.Add(new Carta(newNaipe, sorteio));
                            pics[i].Image = cartas[i].AdicionarImagem(newNaipe, sorteio);
                            repetidas.Add(newNaipe.ToString() + sorteio.ToString());
                            achou = true;
                            cartasImagem.Add(pics[i].Image);

                        }
                    }
                }
            }

            todasRepetidas = cartas[3].Valor == cartas[4].Valor && cartas[3].Valor == cartas[5].Valor; // Verifica se todas as cartas da I.A são iguais
            VirarCartasIA();
        }

        // Vira as cartas que pertencem à I.A para baixo:

        private void VirarCartasIA()
        {
            for (int i = 3; i <= 5; i++)
            {

                pics[i].Image = Properties.Resources.verso;

            }

        }

        // Permite o jogador escolher uma carta da mão para jogar:

        private void Card(object sender, EventArgs e)
        {

            var pic = (PictureBox)sender;
            var tabIndex = int.Parse(String.Format("{0}", pic.Tag));

            if (vez && (tabIndex >= 0 && tabIndex <= 2) && cartas[tabIndex].Mao)
            {
                if (!cartaVirada)
                {
                    valorJogado[0] = cartas[tabIndex].Valor;
                    pic.Image = cartasImagem[tabIndex];
                    cartas[tabIndex].Mao = false;
                    indice = tabIndex;
                    jogadas++;
                }
                else
                {

                    valorJogado[0] = 0;
                    cartas[tabIndex].Mao = false;
                    indice = tabIndex;
                    jogadas++;

                }


                pic.Location = new Point(X, Y);

                if (jogadas == 2)
                {
                    VerificarVencedor(indice2);
                }
                else
                {

                    vez = false;
                    AnalisarTruco();
                    JogadaIARodada1();
                }

            }

        }

        // Faz a IA pensar qual carta é melhor de jogar nas rodadas:

        private void JogadaIARodada1()
        {
            virar = false;
            IAJogada = false;
            var menor = 0;
            var maior = 0;
            var meio = 0;

            if (!vez) // Verifica se está na vez da I.A jogar
            {
                if (!todasRepetidas)
                {

                    for (int i = 3; i <= 5; i++) // Percorre todas as cartas da I.A
                    {

                        if (i == 3) // Adiciona o menor valor para a primeira carta da mão da I.A para fugir do zero
                        {

                            menor = cartas[i].Valor;

                        }

                        if (cartas[i].Valor <= menor) // Faz com que a menor carta na mão da I.A seja adicionada à variável correspondente
                        {

                            menor = cartas[i].Valor;
                            indices[0] = cartas.IndexOf(cartas[i]); // Descobre o índice da carta com menor valor dentro do vetor

                        }

                        if (cartas[i].Valor >= maior) // Faz com que a maior carta na mão da I.A seja adicionada à variável correspondente
                        {

                            maior = cartas[i].Valor;
                            indices[1] = cartas.IndexOf(cartas[i]);

                        }

                    }


                    for (int i = 3; i <= 5; i++)
                    {

                        if (cartas[i].Valor > menor && cartas[i].Valor < maior) // Verifica qual é a segunda maior carta na mão da I.A 
                        {

                            meio = cartas[i].Valor;
                            indices[2] = cartas.IndexOf(cartas[i]);
                        }

                        // Se a carta do meio for repetida, adiciona valor e indice à ela:

                        if (meio == 0)
                        {
                            if (cartas[i].Valor == menor && cartas.IndexOf(cartas[i]) != indices[0])
                            {
                                meio = cartas[i].Valor;
                                indices[2] = cartas.IndexOf(cartas[i]);
                            }
                            else if (cartas[i].Valor == maior && cartas.IndexOf(cartas[i]) != indices[1])
                            {

                                meio = cartas[i].Valor;
                                indices[2] = cartas.IndexOf(cartas[i]);

                            }

                        }
                    }

                    for (int i = 0; i <= 3; i++) // Gerencia as escolhas de jogadas da I.A
                    {

                        if (IAJogada) // Se a I.A já jogou, pára o gerenciamento
                        {

                            break;

                        }

                        switch (i)
                        {
                            case 0: if (rodadas == 0) // caso esteja na primeira rodada
                                {

                                    if (valorJogado[0] == 0) // Caso comece com a I.A
                                    {

                                        var jogada = rdn.Next(1, 101) <= 50; // Tem metade de chance de jogar a menor, e metade de jogar a do meio

                                        if (jogada)
                                        {

                                            IAJogar(menor, indices[0]);

                                        }
                                        else
                                        {

                                            IAJogar(meio, indices[2]);

                                        }

                                    }
                                    else
                                    {

                                        IAVerificar(menor, indices[0]); // Se não começa com a I.A, verifica se pode jogar a menor pra vencer

                                    }

                                }
                                else
                                {
                                    // Caso não esteja na primeira rodada, há a preocupação de vitória ou empate do player e pedido de truco
                                    // Nestes casos, dá prioridade em jogar a carta mais alta para vencer a rodada, se não perderá tudo

                                    if ((valorMao > 1 && valorJogado[0] == 0) || (vencedorOrdem.Contains("Player")) || (vencedorOrdem.Contains("Empate")))
                                    {

                                        if (cartas[indices[1]].Mao) // Se a carta mais alta ainda tiver na mão, joga ela
                                        {

                                            IAVerificar(maior, indices[1]);
                                        }
                                        else if (cartas[indices[2]].Mao) // Se a carta do meio ainda tiver na mão, joga ela
                                        {

                                            IAVerificar(meio, indices[2]);
                                        }
                                        else
                                        {

                                            IAVerificar(menor, indices[0]); // Caso as jogadas maiores já tenham sido feitas, só resta jogar a mais fraca (a única que sobrou)
                                        }

                                    }
                                    else
                                    {
                                        IAVerificar(menor, indices[0]); // Caso não tenha esta preocupação, tenta jogar a menor carta

                                    }
                                }

                                break;

                            case 1: IAVerificar(meio, indices[2]); break; // Se a mais fraca não dá conta do player, joga do meio
                            case 2: IAVerificar(maior, indices[1]); break; // Se a do meio não dá conta, tem que jogar a mais forte

                            // Caso vá perder de qualquer jeito com as cartas que tem na mão, não tem jeito de vitória
                            case 3:

                                if (rodadas > 0) { virar = rdn.Next(1, 101) <= 30; } // 30% de chance de jogar a carta que não resolve nada, coberta


                                if (cartas[indices[0]].Mao) { IAJogar(menor, indices[0]); } // Se a menor tá na mão, joga ela
                                else
                                {

                                    if (cartas[indices[2]].Mao) // Se a do meio tá na mão, joga ela
                                    {

                                        IAJogar(meio, indices[2]);
                                    }

                                    else
                                    {
                                        IAJogar(maior, indices[1]); // Se a maior tá na mão, joga ela em último caso (só sobrou ela)
                                    }
                                }
                                break;
                        }
                    }

                }
                else { 
                
                    // Todas as cartas da I.A são iguais, então não importa a jogada que ela faça:

                    while (!IAJogada) {

                        for (int i = 3; i <= 5; i++) {

                            if (cartas[i].Mao) {

                                IAJogar(cartas[i].Valor, i);
                            
                            }
                        
                        }
                    }
                
                }
            }
        }


        // IA verifica se a carta selecionada pode vencer do Player:

        private void IAVerificar(int pValor, int index)
        {

            if (pValor >= valorJogado[0])
            {
                IAJogar(pValor, index);

            }

        }

        // Faz a I.A jogar a carta pensada (que julga ser a melhor escolha):

        private void IAJogar(int valor, int index)
        {

            if (!vez && cartas[index].Mao)
            {
                jogadas++;
                IAJogada = true;
                pics[index].Location = new Point(X, Y1);
                cartas[index].Mao = false;
                cartas[index].Mesa = true;
                vez = true;


                if (virar) // Joga a carta virada para baixo (não valendo nada)
                {

                    pics[index].Image = Properties.Resources.verso;
                    valorJogado[1] = 0;

                }
                else // Joga a carta normalmente
                {
                    valorJogado[1] = valor;
                    pics[index].Image = cartasImagem[index];
                    indice2 = Array.IndexOf(pics, pics[index]);

                }

                if (jogadas == 2)
                {

                    VerificarVencedor(index);
                }
            }
        }

        // Verifica quem venceu a vaza:

        private void VerificarVencedor(int index)
        {

            Application.DoEvents();
            Thread.Sleep(600);

            if (valorJogado[0] > valorJogado[1])
            {
                p1++;
                vencedorOrdem.Add("Player");


            }
            else if (valorJogado[0] < valorJogado[1])
            {
                p2++;
                vencedorOrdem.Add("IA");

            }
            else
            {
                empate++;
                vencedorOrdem.Add("Empate");

            }

            if (vencedorOrdem[vencedorOrdem.Count - 1] == "Player")
            {

                labels[rodadas].ForeColor = Color.Red;

            }
            else if (vencedorOrdem[vencedorOrdem.Count - 1] == "IA")
            {

                labels[rodadas].ForeColor = Color.Blue;

            }
            else
            {

                labels[rodadas].ForeColor = Color.White;

            }


            AtualizarRodada(index);

        }

        // Atualiza a rodada corrente:

        private void AtualizarRodada(int index)
        {

            rodadas++;
            jogadas = 0;
            jaPediu = false;
            pedidoTruco = false;
            valorJogado[0] = 0;

            foreach (var cards in cartas) {

                if (cards.Mesa)
                {
                    cards.Mesa = false;
                }
            
            }

            foreach (var pb in pics)
            {

                var tabIndex = int.Parse(String.Format("{0}", pb.Tag));

                if (index == tabIndex || indice == tabIndex)
                {

                    pb.Visible = false;
                    pb.Enabled = false;

                }

            }

            acabouVaza = false;
            FimRodada();

            if (!acabouVaza)
            {

                SortearVez();
            }

        }

        // Verifica se a rodada terminou:

        private void FimRodada()
        {


            if (rodadas == 2)
            {

                if (p1 == 2 || p2 == 2)
                {

                    vencedor = p1 == 2;
                    AnunciarVencedor();

                }
                else if (empate == 1)
                {

                    vencedor = vencedorOrdem[vencedorOrdem.Count - 1] == "Player" || vencedorOrdem[0] == "Player";
                    AnunciarVencedor();
                }

            }

            if (rodadas == 3)
            {

                if (p1 == 2 || p2 == 2)
                {

                    vencedor = p1 == 2;
                    AnunciarVencedor();

                }
                else if (empate >= 1)
                {

                    vencedor = vencedorOrdem[vencedorOrdem.Count - 1] == "Player" || vencedorOrdem[0] == "Player";
                    AnunciarVencedor();

                }

            }



        }

        // Anuncia na tela o ganhador da rodada:

        private void AnunciarVencedor()
        {

            acabouVaza = true;

            if (empate == 3)
            {
                MessageBox.Show("Rodada empatada, ninguém recebeu pontos!");
            }

            else if (vencedor)
            {

                MessageBox.Show("Você venceu essa rodada!");
                pontos1 += valorMao;
                lblPontos1.Text = "Seus pontos: " + pontos1.ToString();

            }
            else
            {

                MessageBox.Show("Você perdeu essa rodada!");
                pontos2 += valorMao;
                lblPontos2.Text = "I.A pontos: " + pontos2.ToString();

            }

            FimDeJogo();
        }

        // Verifica se o jogo terminou:

        private void FimDeJogo()
        {

            if (pontos1 >= 12)  // Jogador 1 venceu
            {
                MessageBox.Show("Você venceu a partida! Parabéns!");
                pontos1 = pontos2 = 0;
            }
            else if (pontos2 >= 12) { // I.A venceu

                MessageBox.Show("Você perdeu a partida! =(");
                pontos1 = pontos2 = 0;
            }
            else if (pontos1 >= 12 && pontos2 >= 12) { // Empate

                MessageBox.Show("Isso não costuma ocorrer, mas a partida ficou empatada! Quem vencer agora leva!");
            }

            lblPontos1.Text = "Seus pontos: " + pontos1.ToString();
            lblPontos2.Text = "IA pontos: " + pontos2.ToString();

           ReiniciarPartida();

        }

        // Reinicia a partida por completo:

        private void ReiniciarPartida()
        {

            p1 = p2 = empate = rodadas = jogadas = 0;
            valorMao = 1;
            lblValor.Text = "Valor da aposta: " + valorMao.ToString();
            cartasImagem.Clear();
            cartas.Clear();
            repetidas.Clear();
            vencedorOrdem.Clear();
            cartaVirada = jaPediu = pedidoTruco = todasRepetidas = false;
            btnTruco.Enabled = true;
            chanceTruco = 0.0f;

            foreach (var lbl in labels)
            {

                lbl.ForeColor = Color.DimGray;

            }

            foreach (var pic in pics)
            {

                pic.Enabled = true;
                pic.Visible = true;
            }

            // Coloca as cartas de novo na posição de mão
            pictureBox1.Location = new Point(202, 326);
            pictureBox2.Location = new Point(251, 326);
            pictureBox3.Location = new Point(300, 326);
            pictureBox4.Location = new Point(300, 74);
            pictureBox5.Location = new Point(251, 74);
            pictureBox6.Location = new Point(202, 74);

            SortearVira();
            SortearVez();

            if (!vez)
            {
                AnalisarTruco();
                JogadaIARodada1();
            }

        }

        // Analisa a possibilidade da IA aceitar/negar ou pedir truco:
        private void AnalisarTruco()
        {
            chanceTruco = 0.0f;

            if (!jaPediu)
            {

                if (vencedorOrdem.Contains("IA"))
                { // Aumenta a chance de aceitar se já ganhou alguma rodada

                    chanceTruco += 0.20f;

                }
                else if (vencedorOrdem.Contains("Empate"))
                { // Aumenta a chance de aceitar se empatou

                    chanceTruco += 0.10f;

                }

                for (int i = 3; i <= 5; i++) // Verifica todas as cartas da I.A
                {
                    if ((cartas[i].Valor == 12 || cartas[i].Valor == 13) && (cartas[i].Mao || cartas[i].Mesa)) // Chance de aceitar truco se possui o 3 ou 2 
                    {

                        chanceTruco += 0.15f;
                    }
                    else if ((cartas[i].Valor == 14 || cartas[i].Valor == 15) && (cartas[i].Mao || cartas[i].Mesa)) // Chance de aceitar truco se possui manilhas fracas
                    {

                        chanceTruco += 0.20f;
                    }

                    else if ((cartas[i].Valor == 16 || cartas[i].Valor == 17) && (cartas[i].Mao || cartas[i].Mesa)) // Chance de aceitar truco se possui manilhas fortes
                    {

                        chanceTruco += 0.30f;
                    }

                }

                // IA Pedir truco:

                if (chanceTruco >= 0.30) // Chance com certeza da IA pedir/aceitar um truco.
                {

                    if (!pedidoTruco) // Se não pediram, vai pedir.
                    {
                        if (rodadas > 0 && valorMao < 12)
                        {
                            PedidoTruco(valorMao);
                        }

                    }
                    else
                    {
                        // Se já pediram, aceita!

                        var aumentarTruco = rdn.Next(1, 101) <= 30; // Pode dobrar o valor

                        if (aumentarTruco && valorMao < 12)
                        {

                            PedidoTruco(valorMao);

                        }
                        else
                        {

                            MessageBox.Show("Pedido aceito!!"); // Ou só aceita
                            jaPediu = true;

                        }

                    }
                }
                else
                {

                    // Caso não tenha chance de vencer, ela pede truco por puro blefe com 5% de chance disso ocorrer.

                    var blefeIA = rdn.Next(1, 101) <= 5;

                    if (blefeIA)
                    {

                        if (!pedidoTruco && valorMao < 12) // Pede por blefe
                        {

                            PedidoTruco(valorMao);

                        }

                    }
                    else if (pedidoTruco)
                    { // 30% de chance de aceitar o truco para tirar a prova

                        var aceitar = rdn.Next(1, 101) <= 30;

                        if (!aceitar)
                        {
                            RecusarAposta(ref valorMao);
                            pontos1 += valorMao;
                            MessageBox.Show("Pedido recusado!"); // Se não tiver chance de blefar nem de vencer, recusa o truco.
                            pontos1++;
                            pontos1--;
                            lblPontos1.Text = "Seus pontos: " + pontos1.ToString();
                            vez = true;
                            FimDeJogo();
                        }
                        else
                        {
                            var dobrar = rdn.Next(1, 101) <= 20; // Dobra pra tirar a prova

                            if (dobrar && valorMao < 12)
                            {

                                PedidoTruco(valorMao);
                            }

                            else
                            {

                                MessageBox.Show("Pedido aceito!!"); // Aceita pra tirar a prova
                                jaPediu = true;
                            }

                        }

                    }

                }
            }
        }

        // Faz a I.A pedir TRUCOOO:

        private void PedidoTruco(int mao)
        {
                jaPediu = true;
                audios[1].Play();
                var mensagem = "";
                AnalisarAposta(valorMao);

                Thread.Sleep(100);


                switch (mao)
                {

                    case 1: mensagem = pedidosTruco[rdn.Next(0, pedidosTruco.Length)];
                        break;

                    case 3: mensagem = pedido6[rdn.Next(0, pedido6.Length)];
                        break;

                    case 6: mensagem = pedido9[rdn.Next(0, pedido9.Length)];
                        break;

                    case 9: mensagem = pedido12[rdn.Next(0, pedido12.Length)];
                        break;

                }

                MessageBox.Show(mensagem, "Solicitação de truco do oponente");
                DialogResult acc = MessageBox.Show("Aceitar?", "Solicitação de truco do oponente", MessageBoxButtons.YesNo);

                if (acc == DialogResult.Yes)
                {

                    if (valorMao < 12)
                    {
                        DialogResult resp = MessageBox.Show("Deseja aumentar a aposta?", "Apostar", MessageBoxButtons.YesNo);

                        if (resp == DialogResult.Yes)
                        {

                            Trucar(valorMao);

                        }
                        else
                        {

                            MessageBox.Show("Pedido aceito!");

                        }
                    }

                }
                else
                {
                    RecusarAposta(ref valorMao);
                    pontos2 += valorMao;
                    pontos2--;
                    MessageBox.Show("Pedido recusado");
                    pontos2++;
                    lblPontos2.Text = "I.A pontos: " + pontos2.ToString();
                    vez = true;
                    FimDeJogo();
                }
        }

        // Faz as suas cartas virarem:

        private void btnVirar_Click(object sender, EventArgs e)
        {
            if (rodadas > 0)
            {

                cartaVirada = !cartaVirada;

                for (int i = 0; i <= 2; i++)
                {

                    if (cartaVirada)
                    {

                        pics[i].Image = Properties.Resources.verso;

                    }
                    else
                    {

                        pics[i].Image = cartasImagem[i];

                    }

                }

            }
            else
            {

                MessageBox.Show("Você não pode virar sua carta na primeira rodada!");

            }
        }

        // Botão de pedir truco:

        private void btnTruco_Click(object sender, EventArgs e)
        {
            if (vez && valorMao < 12)
            {


                Trucar(valorMao);
            }

            else
            {

                MessageBox.Show(String.Format("É necessário estar na sua vez para apostar \n e o valor da aposta deve ser menor que a aposta máxima!"));
            }

        }

        // Faz com que o player 1 peça truco ou aumente a aposta
        private void Trucar(int mao) 
        {
            var mensagem = "";
            audios[0].Play();
            pedidoTruco = true;
            jaPediu = false;
            btnTruco.Enabled = false;
            AnalisarAposta(valorMao);

            switch (mao)
            {
                case 1: mensagem = pedidosTruco[rdn.Next(0, pedidosTruco.Length)];
                    break;
                case 3: mensagem = pedido6[rdn.Next(0, pedido6.Length)];
                    break;
                case 6: mensagem = pedido9[rdn.Next(0, pedido9.Length)];
                    break;
                case 9: mensagem = pedido12[rdn.Next(0, pedido12.Length)];
                    break;
            }

            MessageBox.Show(mensagem, "Sua solicitação de truco");
            Thread.Sleep(670);
            AnalisarTruco();
        }

        // Analisa o valor corrente da aposta
        private void AnalisarAposta(int mao) 
        {
            switch (mao)
            {

                case 1: valorMao = 3; break;
                case 3: valorMao = 6; break;
                case 6: valorMao = 9; break;
                case 9: valorMao = 12; break;
            }

            lblValor.Text = "Valor da aposta: " + valorMao.ToString();
        }

        // Faz com que quando a aposta seja recusada, acrescente o valor inferior à aposta anterior aos pontos de quem solicitou truco

        private void RecusarAposta(ref byte Mao) { 
            
            switch (Mao) {

                case 3: Mao = 1;break;
                case 6: Mao = 3; break;
                case 9: Mao = 6; break;
                case 12: Mao = 9; break;
            
            }
        
        }

        // Coloca o jogo em fullscreen
        private void Form1_Load(object sender, EventArgs e) 
        {
            WindowState = FormWindowState.Maximized;
        }
    }
    }

