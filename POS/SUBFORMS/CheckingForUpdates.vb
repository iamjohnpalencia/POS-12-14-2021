Imports System.Threading

Public Class CheckingForUpdates
    Private Shared _instance As CheckingForUpdates
    Public CheckingUpdatesUPDATED As Boolean = False

    Public ReadOnly Property Instance As CheckingForUpdates
        Get
            Return _instance
        End Get
    End Property
    Private Sub CheckingForUpdates_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            _instance = Me

            Timer1.Start()
            CheckForIllegalCrossThreadCalls = False
            BackgroundWorker1.WorkerReportsProgress = True
            BackgroundWorker1.WorkerSupportsCancellation = True
            BackgroundWorker1.RunWorkerAsync()
            ChangeProgBarColor(ProgressBar1, ProgressBarColor.Yellow)
        Catch ex As Exception
            SendErrorReport(ex.ToString)
        End Try
    End Sub

    Private Sub CheckingForUpdates_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If CheckingUpdatesUPDATED Then
                If BegBalanceBool = False Then

                    With POS
                        If .DataGridView1.Rows.Count > 0 Or .DataGridView2.Rows.Count > 0 Or .DataGridView3.Rows.Count > 0 Or .DataGridView4.Rows.Count > 0 Or .PriceChangeDatatabe.Rows.Count > 0 Or .CouponDatatable.Rows.Count > 0 Or .CustomProductsApproval.Rows.Count Or .DataGridView5.Rows.Count > 0 Or .DataGridViewPartners.Rows.Count > 0 Then

                        Else
                            BegBalance.Show()
                            BegBalance.TopMost = True
                            BegBalanceBool = True
                        End If
                    End With
                Else
                    POS.Enabled = True
                End If
            Else
                If ValidCloudConnection Then
                    e.Cancel = True
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    Private Sub GetRowCount()
        Try
            Dim Products = count("product_id", "loc_admin_products")
            Dim Category = count("category_id", "loc_admin_category")
            Dim Inventory = count("inventory_id", "loc_pos_inventory")
            Dim Formula = count("formula_id", "loc_product_formula")
            Dim Coupons = count("ID", "tbcoupon")
            Dim Partners = count("id", "loc_partners_transaction")
            DataGridViewUpdate.Rows.Add(Products)
            DataGridViewUpdate.Rows.Add(Category)
            DataGridViewUpdate.Rows.Add(Inventory)
            DataGridViewUpdate.Rows.Add(Formula)
            DataGridViewUpdate.Rows.Add(Coupons)
            DataGridViewUpdate.Rows.Add(Partners)
        Catch ex As Exception
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            If LabelCheckingUpdates.Text = "Checking for updates." Then
                LabelCheckingUpdates.Text = "Checking for updates.."
            ElseIf LabelCheckingUpdates.Text = "Checking for updates.." Then
                LabelCheckingUpdates.Text = "Checking for updates..."
            ElseIf LabelCheckingUpdates.Text = "Checking for updates..." Then
                LabelCheckingUpdates.Text = "Checking for updates."
            End If
        Catch ex As Exception
            SendErrorReport(ex.ToString)
        End Try

    End Sub
    Dim threadList As List(Of Thread) = New List(Of Thread)
    Dim thread As Thread
    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            thread = New Thread(AddressOf GetRowCount)
            thread.Start()
            threadList.Add(thread)

            For Each t In threadList
                t.Join()
            Next

        Catch ex As Exception
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Try
            ProgressBar1.Maximum = SumOfColumnsToInt(DataGridViewUpdate, 0)

        Catch ex As Exception
            SendErrorReport(ex.ToString)
        End Try
    End Sub
End Class