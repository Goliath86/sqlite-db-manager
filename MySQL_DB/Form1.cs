using System.Data.OleDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace WindowsFormsApplication1
{
    
    public partial class Form1 : Form
    {
        //Dichiarazioni variabili globali
        //private ADOX.Catalog creatore  = new ADOX.Catalog(); // Crea un nuovo oggetto 'creatore' di tipo ADOX.Catalog
        //private System.Data.OleDb.OleDbConnection connessione;
        //private System.Data.OleDb.OleDbCommand com;
        //private System.Data.OleDb.OleDbDataAdapter selezione;
        private SQLiteConnection connessione;
        private SQLiteCommand com;
        private SQLiteDataAdapter selezione;
        private string nomeTable;
        private int tabelle = 0;

        private void Aggiorna_Lista_Tabelle()
        {
            DataTable userTables = null;
            string[] restrictions = new string[] { null, null, null, "Table" }; //Lista di restrizioni per la ricerca di tutte le tabelle presenti in un database specificato
            int i;
            object nomeTemp = "";

            connessione.Open();
            userTables = connessione.GetSchema("Tables", restrictions);
            connessione.Close();

            if (!(comboBox1.SelectedItem == null))
            {
                nomeTemp = comboBox1.SelectedItem; //Memorizza la tabella selezionata attualmente nel comboBox1
            }

            comboBox1.ResetText();
            comboBox1.Items.Clear(); //Cancello tutti gli elementi del ComboBox1 per evitare duplicazioni nell'inserimento dei nuovi elementi
            //dataSet1.Reset();        //Preparo il dataSet1 all'update dei valori visualizzati
            
            //dataGridView1.DataSource = dataSet1;
            
            tabelle = userTables.Rows.Count;
            toolStripStatusLabel2.Text = "Tabelle: " + tabelle.ToString(); //Aggiorna il numero di tabelle presenti nel database

            //Aggiungi al comboBox1 tutti i nomi delle tabelle del database selezionato
            for (i = 0; i < tabelle; i++)
            {
                comboBox1.Items.Add(userTables.Rows[i][2].ToString());
            }

            //Se la tabella precedentemente selezionata nel comboBox1 è ancora presente
            if (comboBox1.Items.Contains(nomeTemp))
            {
                comboBox1.SelectedItem = nomeTemp;
            }
            else
            {
                //altrimenti svuota DGV
                dataSet1.Reset();
                dataGridView1.DataSource = dataSet1;
            }
        }
        
        public Form1()
        {
            InitializeComponent();
        }

        private void esciToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Chiusura applicazione
            this.Close();
        }

        private void nuovoDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Creazione nuovo database
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    //Crea un nuovo database nella directory dell'eseguibile dell'applicazione con il nome assegnato tramite l'openFileDialog1
                    //creatore.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + saveFileDialog1.FileName + ";Jet OLEDB:Engine Type=5");
                    SQLiteConnection.CreateFile("Data Source = " + saveFileDialog1.FileName);
                }
                catch
                {
                    //Eccezione lanciata se database già presente
                }
                
                try
                {
                    //Crea una connessione col database appena creato
                    connessione = new SQLiteConnection("Data Source=" + saveFileDialog1.FileName);
                    toolStripStatusLabel1.Image = SQLite_DBM.Properties.Resources.Green_Ball;
                    toolStripStatusLabel1.Text = "Database corrente: " + System.IO.Path.GetFileName(saveFileDialog1.FileName);
                    Aggiorna_Lista_Tabelle();
                    
                    textBox3.ReadOnly = false; //Abilita l'immissione di comandi
                    textBox3.ResetText();      //Resetta il campo 'Testo' del textBox3
                    textBox3.Select();         //Imposta il focus sulla casella per l'inserimento dei comandi
                }
                catch
                {
                    MessageBox.Show("Errore creazione database!", "ERRORE CREAZIONE DB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    toolStripStatusLabel1.Text = "Nessun database caricato!";
                    toolStripStatusLabel1.Image = SQLite_DBM.Properties.Resources.Red_Ball;
                    textBox3.ReadOnly = true; //Blocca la possibilità di immettere comandi nel textBox3
                    textBox3.Text = "Nessun database caricato";
                    button1.Enabled = false; //Bottone di 'Update' disabilitato
                    button4.Enabled = false; //Bottone 'Salva' disabilitato
                }
            }
        }

        private void caricaDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Caricamento database esistente
            string paths = "";
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Memorizza nella variabile 'paths' il percorso del database da aprire
                paths = openFileDialog1.FileName;
                
                //try
                //{
                    //Crea una connessione col database appena aperto
                    connessione = new SQLiteConnection("Data Source=" + paths);
                    
                    //Prova di connessione col database
                    connessione.Open();
                    connessione.Close();
                    
                    //Se la connessione ha avuto successo
                    toolStripStatusLabel1.Text = "Database corrente: " + System.IO.Path.GetFileName(paths);
                    toolStripStatusLabel1.Image = SQLite_DBM.Properties.Resources.Green_Ball;
                //}
                //catch
                //{
                //    MessageBox.Show("Errore di connessione col database: " + paths, "ERRORE CONNESSIONE AL DB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    toolStripStatusLabel1.Text = "Errore di connessione";
                //    connessione.Close();
                //    toolStripStatusLabel1.Image = SQLite_DBM.Properties.Resources.Red_Ball;
                //    button1.Enabled = false; //Bottone di 'Update' disabilitato
                //    button4.Enabled = false; //Bottone 'Salva' disabilitato
                //    return; //Esci dalla funzione
                //}

                //Se l'apertura del database è avvenuta con successo allora continua
                Aggiorna_Lista_Tabelle();
                
                textBox3.ReadOnly = false; //Permetti l'immissione di comandi nel textBox3
                textBox3.ResetText();      //Resetta il testo del textBox3
                textBox3.Select();         //Imposta il focus sulla casella per l'inserimento dei comandi
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox3.ReadOnly) { return; } //Se non è stato caricato alcun database esci

            //Matrice di stringhe contenente i vari comandi passati nel textBox3, separati da un punto e virgola
            string[] comando;

            //Variabile che contiene il comando dato 'in pasto' al database
            string comando_attuale = "";

            try
            {
                //Dividi i comandi immessi nel textBox3 ed inseriscili nell'array di stringhe 'comando'
                comando = textBox3.Text.Split(';');
            }
            catch (Exception exception)
            {
                MessageBox.Show("Errore esecuzione comando!", "ERRORE COMANDO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox1.Image = SQLite_DBM.Properties.Resources.Red_Ball;
                pictureBox1.Visible = true;
                label5.Text = "Errore esecuzione comando!";
                label5.Visible = true;
                //L'errore viene scritto nel tooltip del pallino rosso visualizzato per poter essere letto se si vuole
                toolTip1.SetToolTip(pictureBox1, exception.ToString());
                return;
            }

            //Esecuzione del comando scritto nel textBox3
            //textBox3.Text = textBox3.Text.Trim();    //Elimina gli spazi vuoti all'inizio ed alla fine della stringa

            for (int i = 0; i < comando.Length; i++)
            {
                comando_attuale = comando[i].Trim();

                if (comando_attuale != "")    //Se il comando non è nullo
                {
                    //Esecuzione comando scritto in textBox3
                    try
                    {
                        for (int k = 0; k <= numericUpDown1.Value; k++) //Esecuzione del comando in base al numero di volte immesse nel controllo numericUpDown1
                        {
                            com = new SQLiteCommand(comando_attuale, connessione); //Prepara il comando
                            connessione.Open();       //Apri la connessione con il database
                            com.ExecuteNonQuery();    //Esegui comando SQL
                            connessione.Close();      //Chiudi la connessione con il database
                            pictureBox1.Image = SQLite_DBM.Properties.Resources.Green_Ball;
                            pictureBox1.Visible = true;
                            label5.Text = "Comando eseguito correttamente!";
                            label5.Visible = true;

                            //Update della lista tabelle e del DGV
                            //var elemento = comboBox1.SelectedItem;
                            Aggiorna_Lista_Tabelle(); //Per eventuale comando passato di aggiunta/eliminazione tabella                  
                            //comboBox1.SelectedItem = elemento;
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Errore esecuzione comando " + comando_attuale, "ERRORE COMANDO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        pictureBox1.Image = SQLite_DBM.Properties.Resources.Red_Ball;
                        pictureBox1.Visible = true;
                        label5.Text = "Errore esecuzione comando!";
                        label5.Visible = true;
                        //L'errore viene scritto nel tooltip del pallino rosso visualizzato per poter essere letto se si vuole
                        toolTip1.SetToolTip(pictureBox1, exception.ToString());
                    }
                    finally
                    {
                        //Chiudi la connessione con il database
                        connessione.Close();
                    }
                }
                textBox3.Select(); //Imposta il focus sulla casella per l'inserimento dei comandi
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Reset del campo del textBox3
            if (textBox3.ReadOnly) { return; } //Se non è stato caricato alcun database esci
            
            textBox3.ResetText();
            textBox3.Select();         //Imposta il focus sulla casella per l'inserimento dei comandi
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            //Rendi invisibile pallino verde e scritta; resetta il tooltip del pallino
            label5.Visible = false;
            pictureBox1.Visible = false;
            toolTip1.SetToolTip(pictureBox1, "");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (!(comboBox1.SelectedItem == null))
            {
                nomeTable = comboBox1.SelectedItem.ToString(); //Memorizza la tabella correntemente selezionata nel comboBox1
                //Visualizza la tabella selezionata nel DGV
                dataSet1.Reset();
                connessione.Open();
                selezione = new SQLiteDataAdapter("SELECT * FROM " + nomeTable, connessione);
                selezione.Fill(dataSet1, nomeTable);
                connessione.Close();
                dataGridView1.DataSource = dataSet1.Tables[0];
            }
            else
            {
                dataSet1.Reset();
                dataGridView1.DataSource = dataSet1;
            }

            button1.Enabled = true; //Abilita bottone 'Update'
            textBox3.Select();      //Imposta il focus sulla casella per l'inserimento dei comandi
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) { textBox3.Select();  return; } //Esci se non è selezionata alcuna tabella
            
            //Salvataggio modifiche al database
            nomeTable = comboBox1.SelectedItem.ToString(); //Memorizza la tabella correntemente selezionata nel comboBox1

            //Salvataggio modifiche manuali al database
            try
            {  
                connessione.Open();
                selezione.SelectCommand = new SQLiteCommand("SELECT * FROM " + nomeTable, connessione);
                SQLiteCommandBuilder cb = new SQLiteCommandBuilder(selezione);
                selezione.Update(dataSet1, nomeTable);
                connessione.Close();
                button1.PerformClick();  //Creo un update video del database nel dataGridView1
                textBox3.Select();       //Imposta il focus sulla casella per l'inserimento dei comandi
            }
            catch(Exception exception)
            {
                MessageBox.Show("Errore salvataggio modifiche del database! Assicurarsi che contenga almeno una colonna chiave", "ERRORE SALVATAGGIO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox1.Image = SQLite_DBM.Properties.Resources.Red_Ball;
                pictureBox1.Visible = true;
                label5.Text = "Errore salvataggio modifiche!";
                label5.Visible = true;
                toolTip1.SetToolTip(pictureBox1, exception.ToString());
                connessione.Close();
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //Abilita salvataggio
            button4.Enabled = true;

            //Reset messaggio
            pictureBox1.Visible = false;
            label5.Visible = false;
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            //Abilita salvataggio
            button4.Enabled = true;

            //Reset messaggio
            pictureBox1.Visible = false;
            label5.Visible = false;
        }

        private void comandoTRUNCATETABLEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Il comando MySQL 'TRUNCATE TABLE table_name' in SQLite non esiste. Al suo posto troviamo la sequenza di due comandi:\n\n DELETE FROM table_name\n\n DELETE FROM sqlite_sequence WHERE name='table_name'\n\n Il primo comando cancella tutti i dati presenti nella tabella table_name mentre il secondo resetta la variabile autoincrementante", "TRUNCATE TABLE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
