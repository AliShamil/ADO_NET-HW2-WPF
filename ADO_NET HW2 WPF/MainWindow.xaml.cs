using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlzEx.Standard;
using Microsoft.VisualBasic;

namespace ADO_NET_HW2_WPF;

public partial class MainWindow : Window
{
    SqlConnection? conn = null;
    DataTable table;
    SqlDataAdapter adapter;
    SqlCommandBuilder cb;

    public MainWindow(string? connectionStr)
    {
        InitializeComponent();
        DataContext = this;


        conn = new SqlConnection(connectionStr);
        adapter = new("SELECT * FROM Authors", conn);
        cb = new(adapter);
        table = new();
    }



    private void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        table.Clear();

        SqlCommand cmd_Search = new SqlCommand("SELECT * FROM Authors \r\nWHERE Id LIKE LOWER(@text) OR LOWER(FirstName)  LIKE LOWER(@text) OR LOWER(LastName) LIKE LOWER(@text) OR LOWER(FirstName + ' ' + LastName) LIKE LOWER(@text)", conn);
        cmd_Search.Parameters.AddWithValue("@text", "%" + Txt_Search.Text + "%");
        adapter.SelectCommand = cmd_Search;
        adapter.Fill(table);

        Authors_Table.ItemsSource = table.AsDataView();

        adapter.SelectCommand.CommandText = "SELECT * FROM Authors";
        
        if(Txt_Search.Text.Length == 0)
            table.Clear();
    }

    private void Btn_Fill_Click(object sender, RoutedEventArgs e)
    {
        if (Txt_Search.Text.Length > 0)
            Txt_Search.Clear();

        table.Clear();

        adapter?.Fill(table);

        Authors_Table.ItemsSource = table.AsDataView();
    }

    private void Btn_Update_Click(object sender, RoutedEventArgs e)
    {
        #region Insert with Raw Sql

        SqlCommand cmd_Insert = new SqlCommand("INSERT Authors VALUES(@id, @firstName, @lastname)", conn);
        cmd_Insert.Parameters.Add("id", SqlDbType.Int);
        cmd_Insert.Parameters["id"].SourceVersion = DataRowVersion.Current;
        cmd_Insert.Parameters["id"].SourceColumn = "Id";

        cmd_Insert.Parameters.Add("firstName", SqlDbType.NVarChar);
        cmd_Insert.Parameters["firstName"].SourceVersion = DataRowVersion.Current;
        cmd_Insert.Parameters["firstName"].SourceColumn = "FirstName";

        cmd_Insert.Parameters.Add("lastName", SqlDbType.NVarChar);
        cmd_Insert.Parameters["lastName"].SourceVersion = DataRowVersion.Current;
        cmd_Insert.Parameters["lastName"].SourceColumn = "LastName";

        adapter.InsertCommand = cmd_Insert;
        #endregion

        #region Upadate with Stored Procedure

        #region StoredProcedure
        //CREATE PROCEDURE usp_UpdateAuthor
        //@id int, @firstName nvarchar(15), @lastname nvarchar(25)
        //AS
        //BEGIN
        //UPDATE Authors SET FirstName = @firstName, LastName = @lastname WHERE Id = @id
        //END
        #endregion

        SqlCommand cmd_Update = new()
        {
            CommandText = "usp_UpdateAuthor",
            CommandType = CommandType.StoredProcedure,
            Connection = conn
        };

        cmd_Update.Parameters.Add("id", SqlDbType.Int);
        cmd_Update.Parameters["id"].SourceVersion = DataRowVersion.Original;
        cmd_Update.Parameters["id"].SourceColumn = "Id";


        cmd_Update.Parameters.Add("firstName", SqlDbType.NVarChar);
        cmd_Update.Parameters["firstName"].SourceVersion = DataRowVersion.Current;
        cmd_Update.Parameters["firstName"].SourceColumn = "FirstName";

        cmd_Update.Parameters.Add("lastName", SqlDbType.NVarChar);
        cmd_Update.Parameters["lastName"].SourceVersion = DataRowVersion.Current;
        cmd_Update.Parameters["lastName"].SourceColumn = "LastName";

        adapter.UpdateCommand = cmd_Update;
        #endregion


        try
        {
            adapter.Update(table);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void Btn_Delete_Click(object sender, RoutedEventArgs e)
    {
        //Command Builder
        DataRow? row = (Authors_Table.SelectedItem as DataRowView)?.Row;

        if (row is null)
            return;

        row.Delete();

        adapter.Update(table);
    }

    private void Btn_Clear_Click(object sender, RoutedEventArgs e)
    {
        Txt_Search.Clear();
        table.Clear();
    }
}
