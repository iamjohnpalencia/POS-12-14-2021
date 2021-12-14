﻿Option Explicit On
Imports MySql.Data.MySqlClient
Imports System.Drawing.Printing
Public Class Inventory
    Private Shared _instance As Inventory
    Public ReadOnly Property Instance As Inventory
        Get
            Return _instance
        End Get
    End Property
    Dim boolinventory As Boolean = False
    Dim prodid As String
    Dim tbl As String
    Dim flds As String
    Private Sub Inventory_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _instance = Me
        Try
            TabControl1.TabPages(0).Text = "Stock Inventory"
            TabControl1.TabPages(1).Text = "Critical Stock"
            TabControl1.TabPages(2).Text = "Fast Moving Stock"
            TabControl1.TabPages(3).Text = "Stock Adjustment"
            TabControl1.TabPages(4).Text = "Stock in (Receiving) Entry"
            TabControl2.TabPages(0).Text = "Product Ingredients(Server)"
            TabControl2.TabPages(1).Text = "Product Ingredients(Local)"
            TabControl5.TabPages(0).Text = "Approved"
            TabControl5.TabPages(1).Text = "Waiting for approval"


            loadinventory()
            loadcriticalstocks()
            loadstockadjustmentreport(False)
            loadfastmovingstock()
            loadstockentry()

            loadinventorycustom()
            loadinventorycustomdisapp()

            If ClientRole = "Crew" Then
                TabControl1.TabPages.Remove(TabControl1.TabPages(4))
                TabControl1.TabPages.Remove(TabControl1.TabPages(3))
                Button7.Enabled = False
                Button7.Visible = False
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Sub loadinventory()
        Try
            fields = "I.product_ingredients as Ingredients, i.sku , CONCAT_WS(' ', ROUND(I.stock_primary,0), F.primary_unit) as PrimaryValue , CONCAT_WS(' ', ROUND(I.stock_secondary,0), F.secondary_unit) as UOM , ROUND(I.stock_no_of_servings,0) as NoofServings, I.stock_status, I.critical_limit, I.date_modified"
            GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:="loc_pos_inventory I INNER JOIN loc_product_formula F ON F.server_formula_id = I.server_inventory_id ", datagrid:=DataGridViewINVVIEW, errormessage:="", successmessage:="", fields:=fields, where:=" I.stock_status = 1 AND I.store_id = " & ClientStoreID & " ORDER BY I.product_ingredients ASC")
            With DataGridViewINVVIEW
                .Columns(0).HeaderCell.Value = "Ingredients"
                .Columns(1).HeaderCell.Value = "SKU"
                .Columns(2).HeaderCell.Value = "Primary"
                .Columns(3).HeaderCell.Value = "UOM"
                .Columns(4).HeaderCell.Value = "No. of Servings"
                .Columns(5).HeaderCell.Value = "Status"
                .Columns(6).HeaderCell.Value = "Critical Limit"
                .Columns(7).HeaderCell.Value = "Date Created"
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Sub loadinventorycustom()
        Try
            fields = "I.product_ingredients as Ingredients, CONCAT_WS(' ', ROUND(I.stock_primary,0), F.primary_unit) as PrimaryValue , CONCAT_WS(' ', I.stock_secondary, F.secondary_unit) as UOM , ROUND(I.stock_no_of_servings,0) as NoofServings, I.stock_status, I.critical_limit, I.date_modified"
            GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:="loc_pos_inventory I INNER JOIN loc_product_formula F ON F.formula_id = I.inventory_id ", datagrid:=DataGridViewCustomInvApproved, errormessage:="", successmessage:="", fields:=fields, where:="F.origin = 'Local' AND I.stock_status = 1 AND I.store_id = " & ClientStoreID)
            With DataGridViewCustomInvApproved
                .Columns(3).HeaderCell.Value = "No. of Servings"
                .Columns(4).HeaderCell.Value = "Status"
                .Columns(5).HeaderCell.Value = "Critical Limit"
                .Columns(6).HeaderCell.Value = "Date Modified"
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Sub loadinventorycustomdisapp()
        Try
            fields = "I.product_ingredients as Ingredients, CONCAT_WS(' ', ROUND(I.stock_primary,0), F.primary_unit) as PrimaryValue , CONCAT_WS(' ', I.stock_secondary, F.secondary_unit) as UOM , ROUND(I.stock_no_of_servings,0) as NoofServings, I.stock_status, I.critical_limit, I.date_modified"
            GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:="loc_pos_inventory I INNER JOIN loc_product_formula F ON F.formula_id = I.formula_id ", datagrid:=DataGridViewCustomDisapp, errormessage:="", successmessage:="", fields:=fields, where:="F.origin = 'Local' AND I.stock_status = 0 AND I.store_id = " & ClientStoreID)
            With DataGridViewCustomDisapp
                .Columns(3).HeaderCell.Value = "No. of Servings"
                .Columns(4).HeaderCell.Value = "Status"
                .Columns(5).HeaderCell.Value = "Critical Limit"
                .Columns(6).HeaderCell.Value = "Date Modified"
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Public Sub loadcriticalstocks()
        Try
            fields = "`product_ingredients`, ROUND(`stock_primary`, 0),ROUND(`stock_secondary`, 0) , `critical_limit`, `date_modified`"
            GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:="loc_pos_inventory", datagrid:=DataGridViewCriticalStocks, errormessage:="", successmessage:="", fields:=fields, where:=" stock_status = 1 AND critical_limit >= stock_primary AND store_id = " & ClientStoreID & "  ORDER BY product_ingredients ASC")
            With DataGridViewCriticalStocks
                .Columns(0).HeaderCell.Value = "Product Name"
                .Columns(1).HeaderCell.Value = "Primary Value"
                .Columns(2).HeaderCell.Value = "Secondary Value"
                .Columns(3).HeaderCell.Value = "Critical Limit"
                .Columns(4).HeaderCell.Value = "Date Modified"
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Sub loadfastmovingstock()
        Try
            fields = "`formula_id`, SUM(stock_primary)"
            GLOBAL_SELECT_ALL_FUNCTION(table:="loc_fm_stock GROUP by formula_id ORDER BY `SUM(stock_primary)` DESC", datagrid:=DataGridViewFASTMOVING, fields:=fields)
            For Each row As DataRow In dt.Rows
                row("formula_id") = GLOBAL_SELECT_FUNCTION_RETURN(table:="loc_product_formula", fields:="product_ingredients", returnvalrow:="product_ingredients", values:="formula_id ='" & row("formula_id") & "'")
            Next
            With DataGridViewFASTMOVING
                .Columns(0).HeaderCell.Value = "Product Ingredients"
                .Columns(1).HeaderCell.Value = "Total Stock Quantity"
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
            cloudconn.Close()
        End Try
    End Sub
    'Private Sub loadpanelstockadjustment()
    '    Try
    '        fields = "`formula_id`, `product_ingredients`, ROUND(stock_primary,0) AS P, `stock_secondary`, ROUND(stock_no_of_servings,0) AS S, `server_inventory_id`"
    '        GLOBAL_SELECT_ALL_FUNCTION("`loc_pos_inventory` WHERE `main_inventory_id` = 0 AND `stock_status` = 1 AND `store_id` = " & ClientStoreID & " ORDER BY product_ingredients ASC", fields, DataGridViewPanelStockAdjustment)
    '        With DataGridViewPanelStockAdjustment
    '            .Columns(0).Visible = False
    '            .Columns(1).HeaderText = "Ingredient"
    '            .Columns(2).HeaderText = "Primary Value"
    '            .Columns(3).HeaderText = "Secondary Value"
    '            .Columns(4).HeaderText = "No. of servings"
    '            .Columns(5).Visible = False
    '        End With
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub

    Dim DataTableInventory As New DataTable
    Dim DataTableFormula As New DataTable
    Dim inv
    Public Sub loadstockentry()
        Try
            where = " date(log_date_time) = CURRENT_DATE() AND log_type = 'STOCK ENTRY' "
            fields = "`crew_id`, `log_type`, `log_description`, `log_date_time`"
            GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:="loc_system_logs", datagrid:=DataGridViewSTOCKENTRY, errormessage:="", successmessage:="", fields:=fields, where:=where)
            With DataGridViewSTOCKENTRY
                .Columns(0).HeaderText = "Service Crew"
                .Columns(1).Visible = False
                .Columns(2).HeaderText = "Description"
                .Columns(3).HeaderText = "Date and Time"
                For Each row As DataRow In dt.Rows
                    row("crew_id") = GLOBAL_SELECT_FUNCTION_RETURN(table:="loc_users", fields:="full_name", returnvalrow:="full_name", values:="uniq_id ='" & row("crew_id") & "'")
                Next
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Dim inventoryid
    'Private Sub DataGridViewPanelStockAdjustment_CellClick(sender As Object, e As DataGridViewCellEventArgs)
    '    Try
    '        TextBoxIPrimaryVal.Text = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(2).Value
    '        TextBoxISecondaryTotal.Text = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(3).Value
    '        Dim FormulaID As Integer = 0
    '        Dim Origin As String = ""
    '        If DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(0).Value = 0 Then
    '            FormulaID = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(5).Value
    '            Origin = "Server"
    '        Else
    '            FormulaID = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(0).Value
    '            Origin = "Local"
    '        End If
    '        SelectFormula(FormulaID, Origin)
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub SelectFormula(FormulaID, Origin)
    '    Try
    '        Dim sql As String = ""
    '        If Origin = "Local" Then
    '            sql = "SELECT `primary_unit`, `primary_value`, `secondary_unit`, `secondary_value`, `serving_unit`, `serving_value`, `no_servings` FROM loc_product_formula WHERE formula_id = " & FormulaID
    '        ElseIf Origin = "Server" Then
    '            sql = "SELECT `primary_unit`, `primary_value`, `secondary_unit`, `secondary_value`, `serving_unit`, `serving_value`, `no_servings` FROM loc_product_formula WHERE server_formula_id = " & FormulaID
    '        End If
    '        Dim cmd As MySqlCommand = New MySqlCommand(sql, LocalhostConn)
    '        Dim da As MySqlDataAdapter = New MySqlDataAdapter(cmd)
    '        Dim dt As DataTable = New DataTable
    '        da.Fill(dt)
    '        For Each row As DataRow In dt.Rows
    '            TextBoxIPrimaryUnit.Text = row("primary_unit")
    '            TextBoxFPrimaryVal.Text = row("primary_value")
    '            TextBoxFSecondaryUnit.Text = row("secondary_unit")
    '            TextBoxFSecondary.Text = row("secondary_value")
    '            TextBox7.Text = row("serving_unit")
    '            TextBox8.Text = row("serving_value")
    '            TextBoxFnoofservings.Text = row("no_servings")
    '        Next
    '        LocalhostConn.Close()
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub SelectFormulaEntry(FormulaID, Origin)
    '    Try
    '        Dim sql As String = ""
    '        If Origin = "Local" Then
    '            sql = "SELECT `primary_unit`, `primary_value`, `secondary_unit`, `secondary_value`, `serving_unit`, `serving_value`, `no_servings` FROM loc_product_formula WHERE formula_id = " & FormulaID
    '        ElseIf Origin = "Server" Then
    '            sql = "SELECT `primary_unit`, `primary_value`, `secondary_unit`, `secondary_value`, `serving_unit`, `serving_value`, `no_servings` FROM loc_product_formula WHERE server_formula_id = " & FormulaID
    '        End If
    '        Dim cmd As MySqlCommand = New MySqlCommand(sql, LocalhostConn)
    '        Dim da As MySqlDataAdapter = New MySqlDataAdapter(cmd)
    '        Dim dt As DataTable = New DataTable
    '        da.Fill(dt)
    '        For Each row As DataRow In dt.Rows
    '            TextBoxEFPrimaryVal.Text = row("primary_value")
    '            TextBoxEFPUnit.Text = row("primary_unit")

    '            TextBoxEFSecondVal.Text = row("secondary_value")
    '            TextBoxEFSUnit.Text = row("secondary_unit")

    '            TextBoxEServingValue.Text = row("serving_value")
    '            TextBoxEServingVal.Text = row("serving_unit")
    '            TextBoxENoServings.Text = row("no_servings")
    '        Next
    '        LocalhostConn.Close()
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    Sub loadstockadjustmentreport(searchdate As Boolean)
        Try
            fields = "`crew_id`, `log_type`, `log_description`, `log_date_time`, `log_store`, `guid`, `loc_systemlog_id`, `synced`"
            table = "loc_system_logs"
            If searchdate = False Then
                where = " date(log_date_time) = CURRENT_DATE() AND log_type IN('NEW STOCK ADDED','STOCK REMOVAL','STOCK TRANSFER')"
                GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:=table, datagrid:=DataGridViewStockAdjustment, errormessage:="", fields:=fields, successmessage:="", where:=where)
            Else
                where = " log_type IN('NEW STOCK ADDED','STOCK REMOVAL','STOCK TRANSFER') AND date(log_date_time) >= '" & Format(DateTimePicker1.Value, "yyyy-MM-dd") & "' AND date(log_date_time) <= '" & Format(DateTimePicker2.Value, "yyyy-MM-dd") & "'"
                GLOBAL_SELECT_ALL_FUNCTION_WHERE(table:=table, datagrid:=DataGridViewStockAdjustment, errormessage:="", fields:=fields, successmessage:="", where:=where)
            End If
            With DataGridViewStockAdjustment
                .Columns(0).HeaderText = "Service Crew"
                .Columns(1).HeaderText = "Action"
                .Columns(2).HeaderText = "Description"
                .Columns(3).HeaderText = "Date and Time"
                .Columns(4).Visible = False
                .Columns(5).Visible = False
                .Columns(6).Visible = False
                .Columns(7).Visible = False
                For Each row As DataRow In dt.Rows
                    row("crew_id") = GLOBAL_SELECT_FUNCTION_RETURN(table:="loc_users", fields:="full_name", returnvalrow:="full_name", values:="uniq_id ='" & row("crew_id") & "'")
                Next
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    'Private Sub LoadOutlets()
    '    Try
    '        GLOBAL_SELECT_ALL_FUNCTION_COMBOBOX("admin_outlets", "store_name", ComboBoxtransfer, False)
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub FillComboboxReason()
    '    Try
    '        GLOBAL_SELECT_ALL_FUNCTION_COMBOBOX("loc_transfer_data WHERE active = 1", "transfer_cat", ComboBoxDeduction, True)
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub LoadReasonCategories()
    '    Try
    '        GLOBAL_SELECT_ALL_FUNCTION("`loc_transfer_data` WHERE active = 1", "`transfer_id`, `transfer_cat`, `crew_id`, `created_at`, `created_by`, `updated_at`", DataGridViewReasonCategories)
    '        With DataGridViewReasonCategories
    '            .Columns(0).Visible = False
    '            .Columns(1).HeaderText = "Category"
    '            .Columns(2).HeaderText = "Crew"
    '            .Columns(3).HeaderText = "Date Created"
    '            .Columns(4).HeaderText = "Created By"
    '            .Columns(5).HeaderText = "Updated At"
    '        End With
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
    '    Try
    '        loadpanelstockadjustment()
    '        LoadReasonCategories()
    '        LoadReasonCategoriesDeactivated()
    '        PanelSTOCKADJUSTMENT.Location = New Point(ClientSize.Width / 2 - PanelSTOCKADJUSTMENT.Size.Width / 2, ClientSize.Height / 5 - PanelSTOCKADJUSTMENT.Size.Height / 5)
    '        PanelSTOCKADJUSTMENT.Anchor = AnchorStyles.None
    '        PanelSTOCKADJUSTMENT.Visible = True
    '        countingredients()
    '        Dim arg = New DataGridViewCellEventArgs(0, 0)
    '        DataGridViewPanelStockAdjustment_CellClick(sender, arg)
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub countingredients()
    '    Label7.Text = "(" & count(table:="loc_pos_inventory", tocount:="inventory_id") & ") record(s) count"
    'End Sub
    Dim totalqty As Integer

    Private Sub ButtonENTRYADDSTOCK_Click(sender As Object, e As EventArgs)

    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            If DateTimePicker1.Value.Date > DateTimePicker2.Value.Date Then
                MsgBox("")
            Else
                loadstockadjustmentreport(True)
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    'Private Sub ComboBoxDESC_SelectedIndexChanged(sender As Object, e As EventArgs)
    '    Try
    '        Dim sql = "SELECT inventory_id, stock_primary, stock_secondary, origin FROM loc_pos_inventory WHERE product_ingredients = '" & ComboBoxDESC.Text & "'"
    '        Dim cmd As MySqlCommand = New MySqlCommand(sql, LocalhostConn)
    '        Dim da As MySqlDataAdapter = New MySqlDataAdapter(cmd)
    '        Dim dt As DataTable = New DataTable
    '        da.Fill(dt)
    '        TextBoxEPrimary.Text = dt(0)(1)
    '        TextBoxESecondary.Text = dt(0)(2)
    '        TextBox1.Text = dt(0)(0)
    '        SelectFormulaEntry(dt(0)(0), dt(0)(3))
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub

    'Private Sub ButtonSave_Click(sender As Object, e As EventArgs)
    '    Try
    '        Dim SQL = ""
    '        Dim Ingredient = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(1).Value
    '        Dim Origin As String = ""
    '        Dim ID As Integer = 0
    '        If DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(0).Value = 0 Then
    '            Origin = "Server"
    '            ID = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(5).Value
    '            SQL = "SELECT `stock_primary`, `stock_secondary`, `stock_no_of_servings` FROM  `loc_pos_inventory` WHERE `server_inventory_id` = " & ID
    '        Else
    '            Origin = "Local"
    '            ID = DataGridViewPanelStockAdjustment.SelectedRows(0).Cells(0).Value
    '            SQL = "SELECT `stock_primary`, `stock_secondary`, `stock_no_of_servings` FROM  `loc_pos_inventory` WHERE `inventory_id` = " & ID
    '        End If

    '        If ComboBoxAction.Text <> "" Then
    '            If TextboxIsEmpty(Panel23) Then
    '                Dim Primary As Double = Double.Parse(TextBoxIPQuantity.Text) * Double.Parse(TextBoxFPrimaryVal.Text)
    '                Dim Secondary As Double = Double.Parse(TextBoxIPQuantity.Text) * Double.Parse(TextBoxFSecondary.Text)
    '                Dim NoOfServings As Double = Double.Parse(TextBoxIPQuantity.Text) * Double.Parse(TextBoxFnoofservings.Text)

    '                Dim SQLCmd As MySqlCommand = New MySqlCommand(SQL, LocalhostConn)
    '                Dim SQlDa As MySqlDataAdapter = New MySqlDataAdapter(SQLCmd)
    '                Dim InvDT As DataTable = New DataTable
    '                SQlDa.Fill(InvDT)
    '                Dim InvPrimary As Double = 0
    '                Dim InvSecondary As Double = 0
    '                Dim InvServings As Double = 0
    '                For Each row As DataRow In InvDT.Rows
    '                    InvPrimary = row("stock_primary")
    '                    InvSecondary = row("stock_secondary")
    '                    InvServings = row("stock_no_of_servings")
    '                Next
    '                LocalhostConn.Close()
    '                If ComboBoxAction.Text = "ADD" Then
    '                    Dim TotalPrimary As Double = Primary + InvPrimary
    '                    Dim TotalSecondary As Double = Secondary + InvSecondary
    '                    Dim TotalNoOfServings As Double = NoOfServings + InvServings

    '                    SystemLogType = "NEW STOCK ADDED"
    '                    SystemLogDesc = "Adding stock of: " & Ingredient & " ,Quantity(Primary): " & TextBoxIPQuantity.Text & " ,Reason: " & TextBoxIReason.Text
    '                    Dim table = "loc_pos_inventory"
    '                    Dim fields = "`stock_primary`=" & TotalPrimary & ",`stock_secondary`= " & TotalSecondary & " , `stock_no_of_servings`= " & TotalNoOfServings & ", `date_modified` = '" & FullDate24HR() & "'"
    '                    Dim where = ""
    '                    If Origin = "Local" Then
    '                        where = "formula_id = " & ID
    '                    Else
    '                        where = "server_inventory_id = " & ID
    '                    End If

    '                    GLOBAL_FUNCTION_UPDATE(table, fields, where)
    '                ElseIf ComboBoxAction.Text = "TRANSFER" Then

    '                    Dim TotalPrimary As Double = InvPrimary - Primary
    '                    Dim TotalSecondary As Double = InvSecondary - Secondary
    '                    Dim TotalNoOfServings As Double = InvServings - NoOfServings

    '                    SystemLogType = "STOCK TRANSFER"
    '                    SystemLogDesc = "Transfer stock to: " & ComboBoxtransfer.Text & " ,Item: " & Ingredient & " ,Quantity(Primary): " & TextBoxIPQuantity.Text & " ,Reason: " & TextBoxIReason.Text

    '                    Dim table = "loc_pos_inventory"
    '                    Dim fields = "`stock_primary`=" & TotalPrimary & ",`stock_secondary`= " & TotalSecondary & " , `stock_no_of_servings`= " & TotalNoOfServings & ", `date_modified` = '" & FullDate24HR() & "'"
    '                    Dim where = ""
    '                    If Origin = "Local" Then
    '                        where = "formula_id = " & ID
    '                    Else
    '                        where = "server_inventory_id = " & ID
    '                    End If

    '                    GLOBAL_FUNCTION_UPDATE(table, fields, where)
    '                ElseIf ComboBoxAction.Text = "DEDUCT" Then
    '                    Dim TotalPrimary As Double = InvPrimary - Primary
    '                    Dim TotalSecondary As Double = InvSecondary - Secondary
    '                    Dim TotalNoOfServings As Double = InvServings - NoOfServings

    '                    SystemLogType = "STOCK REMOVAL"
    '                    SystemLogDesc = "Removing stock of: " & Ingredient & " ,Quantity(Primary): " & TextBoxIPQuantity.Text & " ,Reason: (" & ComboBoxDeduction.Text & ") " & TextBoxIReason.Text

    '                    Dim table = "loc_pos_inventory"
    '                    Dim fields = "`stock_primary`=" & TotalPrimary & ",`stock_secondary`= " & TotalSecondary & " , `stock_no_of_servings`= " & TotalNoOfServings & ", `date_modified` = '" & FullDate24HR() & "'"

    '                    Dim where = ""
    '                    If Origin = "Local" Then
    '                        where = "formula_id = " & ID
    '                    Else
    '                        where = "server_inventory_id = " & ID
    '                    End If
    '                    GLOBAL_FUNCTION_UPDATE(table, fields, where)
    '                End If
    '                GLOBAL_SYSTEM_LOGS(SystemLogType, SystemLogDesc)
    '                loadpanelstockadjustment()
    '                loadinventory()
    '                loadstockadjustmentreport(False)
    '                loadcriticalstocks()
    '                MDIFORM.LabelTotalAvailStock.Text = roundsum("stock_primary", "loc_pos_inventory WHERE store_id = " & ClientStoreID & " AND guid = '" & ClientGuid & "'", "P")
    '                MDIFORM.LabelTotalCrititems.Text = count(table:="loc_pos_inventory WHERE stock_status = 1 AND critical_limit >= stock_primary AND store_id ='" & ClientStoreID & "' AND guid = '" & ClientGuid & "'", tocount:="inventory_id")
    '            Else
    '                MessageBox.Show("Fill up all empty fields", "Empty Fields", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    '            End If
    '        Else
    '            MessageBox.Show("Select action first", "No Action Selected", MessageBoxButtons.OK, MessageBoxIcon.Information)
    '        End If

    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub

    'Private Sub ComboBoxAction_SelectedIndexChanged(sender As Object, e As EventArgs)
    '    Try
    '        If ComboBoxAction.Text = "DEDUCT" Then
    '            ComboBoxDeduction.Enabled = True
    '            ComboBoxtransfer.Enabled = False
    '            FillComboboxReason()
    '        ElseIf ComboBoxAction.Text = "TRANSFER" Then
    '            If CheckForInternetConnection() = True Then
    '                ComboBoxDeduction.Enabled = False
    '                ComboBoxtransfer.Enabled = True
    '                LoadOutlets()
    '            Else
    '                MsgBox("Internet connection not available.")
    '                ComboBoxAction.SelectedIndex = 0
    '            End If
    '        Else
    '            ComboBoxDeduction.Enabled = False
    '            ComboBoxtransfer.Enabled = False
    '        End If
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub Button2_Click(sender As Object, e As EventArgs)
    '    PanelSTOCKADJUSTMENT.Visible = False
    'End Sub
    'Dim AddOrUpdate As Boolean = False
    'Private Sub Button5_Click(sender As Object, e As EventArgs)
    '    AddOrUpdate = False
    '    PanelReasonCat.Visible = True
    'End Sub
    'Private Sub Button6_Click(sender As Object, e As EventArgs)
    '    Try
    '        If TextBoxReasonsCat.Text <> "" Then
    '            If AddOrUpdate = False Then
    '                Dim sql = "INSERT INTO `loc_transfer_data`(`transfer_cat`, `crew_id`, `created_at`, `created_by`, `updated_at`, `active`) VALUES (@1,@2,@3,@4,@5,@6)"
    '                cmd = New MySqlCommand(sql, LocalhostConn)
    '                cmd.Parameters.Add("@1", MySqlDbType.Text).Value = TextBoxReasonsCat.Text
    '                cmd.Parameters.Add("@2", MySqlDbType.Text).Value = ClientCrewID
    '                cmd.Parameters.Add("@3", MySqlDbType.Text).Value = FullDate24HR()
    '                cmd.Parameters.Add("@4", MySqlDbType.Text).Value = ClientCrewID
    '                cmd.Parameters.Add("@5", MySqlDbType.Text).Value = FullDate24HR()
    '                cmd.Parameters.Add("@6", MySqlDbType.Int64).Value = "1"
    '                cmd.ExecuteNonQuery()
    '                LoadReasonCategories()
    '                PanelReasonCat.Visible = False
    '                TextBoxReasonsCat.Clear()
    '                GLOBAL_SYSTEM_LOGS("NEW CATEGORIES REASON", "Category Name: " & TextBoxReasonsCat.Text & " ,Added By: " & ClientCrewID)
    '                FillComboboxReason()
    '            Else
    '                Dim sql = "UPDATE `loc_transfer_data` SET `transfer_cat`=@1, `crew_id`=@2, `created_at`=@3, `created_by`=@4, `updated_at`=@5, `active`=@6 WHERE transfer_id = " & DataGridViewReasonCategories.SelectedRows(0).Cells(0).Value
    '                cmd = New MySqlCommand(sql, LocalhostConn)
    '                cmd.Parameters.Add("@1", MySqlDbType.Text).Value = TextBoxReasonsCat.Text
    '                cmd.Parameters.Add("@2", MySqlDbType.Text).Value = ClientCrewID
    '                cmd.Parameters.Add("@3", MySqlDbType.Text).Value = FullDate24HR()
    '                cmd.Parameters.Add("@4", MySqlDbType.Text).Value = ClientCrewID
    '                cmd.Parameters.Add("@5", MySqlDbType.Text).Value = FullDate24HR()
    '                cmd.Parameters.Add("@6", MySqlDbType.Int64).Value = "0"
    '                cmd.ExecuteNonQuery()
    '                LoadReasonCategories()
    '                PanelReasonCat.Visible = False
    '                TextBoxReasonsCat.Clear()
    '                GLOBAL_SYSTEM_LOGS("UPDATE CATEGORIES REASON", "Category Name: " & TextBoxReasonsCat.Text & " ,Updated By: " & ClientCrewID)
    '                FillComboboxReason()
    '            End If
    '        Else
    '            MsgBox("Fill up required fields.")
    '        End If
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub Button7_Click(sender As Object, e As EventArgs)
    '    PanelReasonCat.Visible = False
    'End Sub
    'Private Sub ButtonDeleteProducts_Click(sender As Object, e As EventArgs)
    '    Try
    '        If DataGridViewReasonCategories.SelectedRows.Count > 0 Then
    '            Dim msg = MessageBox.Show("Are you sure do you want to deactivate this category ?", "Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
    '            If msg = DialogResult.Yes Then
    '                Dim sql = "UPDATE loc_transfer_data SET active = 0, updated_at = '" & FullDate24HR() & "' WHERE transfer_id = " & DataGridViewReasonCategories.SelectedRows(0).Cells(0).Value
    '                Dim cmd As MySqlCommand = New MySqlCommand(sql, LocalhostConn)
    '                cmd.ExecuteNonQuery()
    '                LoadReasonCategories()
    '                LoadReasonCategoriesDeactivated()
    '            End If
    '        Else
    '            MsgBox("Select Category first")
    '        End If
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub Button9_Click(sender As Object, e As EventArgs)
    '    Try
    '        If DataGridViewReasonCategories.Rows.Count > 0 Then
    '            TextBoxReasonsCat.Text = DataGridViewReasonCategories.SelectedRows(0).Cells(1).Value
    '            AddOrUpdate = True
    '            PanelReasonCat.Visible = True
    '        Else
    '            MsgBox("Select category first")
    '        End If
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub Button4_Click(sender As Object, e As EventArgs)
    '    PanelSTOCKADJUSTMENT.Visible = False
    'End Sub
    'Private Sub Button11_Click(sender As Object, e As EventArgs)
    '    Try
    '        If DataGridViewDeactivatedReasonCat.SelectedRows.Count = 1 Then
    '            Dim msg = MessageBox.Show("Are you sure do you want to activate this category ?", "Activation", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
    '            If msg = DialogResult.Yes Then
    '                Dim sql = "UPDATE `loc_transfer_data` SET `active`=@1 , `updated_at`=@2 WHERE transfer_id = " & DataGridViewDeactivatedReasonCat.SelectedRows(0).Cells(0).Value
    '                cmd = New MySqlCommand(sql, LocalhostConn)
    '                cmd.Parameters.Add("@1", MySqlDbType.Text).Value = "1"
    '                cmd.Parameters.Add("@2", MySqlDbType.Text).Value = FullDate24HR()
    '                cmd.ExecuteNonQuery()
    '                LoadReasonCategories()
    '                LoadReasonCategoriesDeactivated()
    '                PanelReasonCat.Visible = False
    '                TextBoxReasonsCat.Clear()
    '                GLOBAL_SYSTEM_LOGS("REASON CATEGORY ACTIVATED", "Category Name: " & TextBoxReasonsCat.Text & " ,Activated By: " & ClientCrewID)
    '                FillComboboxReason()
    '            End If
    '        ElseIf DataGridViewDeactivatedReasonCat.SelectedRows.Count > 1 Then
    '            MsgBox("Select one category only.")
    '        Else
    '            MsgBox("Select category Category first")
    '        End If
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub LoadReasonCategoriesDeactivated()
    '    Try
    '        GLOBAL_SELECT_ALL_FUNCTION("`loc_transfer_data` WHERE active = 0", "`transfer_id`, `transfer_cat`, `crew_id`, `created_at`, `created_by`, `updated_at`", DataGridViewDeactivatedReasonCat)
    '        With DataGridViewDeactivatedReasonCat
    '            .Columns(0).Visible = False
    '            .Columns(1).HeaderText = "Category"
    '            .Columns(2).HeaderText = "Crew"
    '            .Columns(3).HeaderText = "Date Created"
    '            .Columns(4).HeaderText = "Created By"
    '            .Columns(5).HeaderText = "Updated At"
    '        End With
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '        SendErrorReport(ex.ToString)
    '    End Try
    'End Sub
    'Private Sub Button14_Click(sender As Object, e As EventArgs)
    '    PanelSTOCKADJUSTMENT.Visible = False
    'End Sub
    Private Sub TextBoxIPQuantity_KeyPress(sender As Object, e As KeyPressEventArgs)
        Try
            If InStr(DisallowedCharacters, e.KeyChar) > 0 Then
                e.Handled = True
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Private Sub Button7_Click_1(sender As Object, e As EventArgs) Handles Button7.Click
        Try
            Dim msg = MessageBox.Show("Are you sure you want to reset the inventory ? Press Yes to continue or No to cancel", "NOTICE", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
            If msg = DialogResult.Yes Then
                Dim sql = "UPDATE loc_pos_inventory SET stock_primary = 0, stock_secondary = 0,  stock_no_of_servings = 0"
                Dim cmd As MySqlCommand = New MySqlCommand(sql, LocalhostConn)
                cmd.ExecuteNonQuery()
                loadinventory()
                SystemLogType = "INVENTORY RESET"
                SystemLogDesc = "Reset by :" & returnfullname(ClientCrewID) & " : " & ClientRole
                GLOBAL_SYSTEM_LOGS(SystemLogType, SystemLogDesc)
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Private WithEvents printdoc As PrintDocument = New PrintDocument
    Private PrintPreviewDialog1 As New PrintPreviewDialog

    Private Sub ButtonPrintCurInv_Click(sender As Object, e As EventArgs) Handles ButtonPrintCurInv.Click
        Dim b = 0
        Dim a = 0
        Try
            a = 100
            b = 0
            For i As Integer = 0 To DataGridViewINVVIEW.Rows.Count - 1 Step +1
                b += 10
            Next

            printdoc.DefaultPageSettings.PaperSize = New PaperSize("Custom", ReturnPrintSize(), 300 + b)
            PrintPreviewDialog1.Document = printdoc
            PrintPreviewDialog1.ShowDialog()
            ' printdoc.Print()
        Catch ex As Exception
            MessageBox.Show("An error occurred while trying to load the " &
                "document for Print Preview. Make sure you currently have " &
                "access to a printer. A printer must be localconnected and " &
                "accessible for Print Preview to work.", Me.Text,
                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Private Sub pdoc_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles printdoc.PrintPage
        Try
            Dim font As New Font("Tahoma", 5)
            Dim font1 As New Font("Tahoma", 5, FontStyle.Bold)
            Dim b = 0
            Dim a = 130
            ReceiptHeader(sender, e, False)
            SimpleTextDisplay(sender, e, "INGREDIENTS", font1, 0, a)
            SimpleTextDisplay(sender, e, "PRIMARY", font1, 70, a)
            SimpleTextDisplay(sender, e, "SERVINGS", font1, 140, a)
            a += 20
            For i As Integer = 0 To DataGridViewINVVIEW.Rows.Count - 1 Step +1
                SimpleTextDisplay(sender, e, DataGridViewINVVIEW.Rows(i).Cells(1).Value, font, 0, a)
                SimpleTextDisplay(sender, e, DataGridViewINVVIEW.Rows(i).Cells(2).Value, font, 70, a)
                SimpleTextDisplay(sender, e, DataGridViewINVVIEW.Rows(i).Cells(3).Value, font, 140, a)
                a += 10
            Next
            CenterTextDisplay(sender, e, "*************************************", font, a + 30)
            CenterTextDisplay(sender, e, Format(Now(), "yyyy-MM-dd HH:mm:ss"), font, a + 50)
        Catch ex As Exception
            MsgBox(ex.ToString)
            SendErrorReport(ex.ToString)
        End Try
    End Sub
    Private Sub ButtonKeyboard_Click(sender As Object, e As EventArgs) Handles ButtonKeyboard.Click
        ShowKeyboard()
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Enabled = False
        StockAdjustment.Show()
    End Sub

    Private Sub Inventory_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Application.OpenForms().OfType(Of PanelReasonCat).Any Then
            PanelReasonCat.Close()
        End If
        If Application.OpenForms().OfType(Of StockAdjustment).Any Then
            StockAdjustment.Close()
        End If
        If Application.OpenForms().OfType(Of NewStockEntry).Any Then
            NewStockEntry.Close()
        End If
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        NewStockEntry.Show()
        Enabled = False
    End Sub
End Class