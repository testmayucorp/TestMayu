Imports Logic = SyukkaRef.SyukkaRef_001
Imports System.Net

Public Class SyukkaRef_000
    Inherits IbrFormBase

    ''' <summary>
    ''' [IN]出荷場所
    ''' </summary>
    Public dgvCount As Integer  'DGVの選択した行のインデックス
    Public intRowNo As Integer  'DGVの選択した行の行番号
    Public gazouPath As String  '画像の保存フォルダ
    Private strFactoryDef As String  '工場の初期値

    Private Const KeyGazouPath As String = "PhotoDir"   '環境テーブルの画像保存パスのキー

    ''' <summary>
    ''' 前回検索条件を保存する
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Property SearchCondition As New Dictionary(Of String, Object)

    ''' <summary>
    ''' Formの初期化
    ''' </summary>
    ''' <param name="onFormLoad"></param>
    Public Overrides Sub InitControl(ByVal onFormLoad As Boolean)
        MyBase.InitControl(onFormLoad)

        '画像ファイルの保存パスを取得　
        gazouPath = SyukkaRef_001.getEnvValue(KeyGazouPath, Ibr)
        DGV.DataSource = Nothing
        btnF7.Enabled = False

        If onFormLoad = True Then
            '■①起動されたＰＣのＩＰアドレスにより工場の特定を行う
            '井原(1)・笠岡(3)・総社(2)・総社第2(4)
            strFactoryDef = IBR_SJK_C050.DataUtil.GetDefaultKojoCode
        End If

        C_ShukkaBasho.SelectedValue = strFactoryDef
    End Sub

#Region "Fキーイベント"
    ''' <summary>
    ''' 表示クリア(F2)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF2_Click(sender As Object, e As EventArgs)
        Dim msgResult = MessageBox.Show(IbrCom.MsgINFO.ERR92, My.Application.Info.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Information)
        If (msgResult = DialogResult.Yes) Then
            InitControl(False)
        End If
        D_YoteiSt.Focus()

    End Sub

    ''' <summary>
    ''' 詳細(F7)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF7_Click(sender As Object, e As EventArgs)
        MyBase.BtnF7_Click(sender, e)
        If DGV.CurrentRow.Index = -1 Then
            Return
        End If
        dgvCount = DGV.CurrentRow.Index

        Using f As New SyukkaRef_010(Ibr, Me, Me.C_ShukkaBasho.SelectedItem.ToString)
            '出荷実績詳細を表示する
            Dim parent = If(Me.ParentForm, Me)
            AddHandler f.Shown, Sub() parent.Hide()
            AddHandler f.FormClosed, Sub() parent.Show()
            f.Size = Size
            Dim ret As DialogResult = f.ShowDialog(Me)
            DGV.Focus()
            DGV.CurrentCell = DGV(DGV.CurrentCell.ColumnIndex, dgvCount)
        End Using
    End Sub

    ''' <summary>
    ''' F7:詳細表示(ダブルクリック)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DGV_Detail_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DGV.MouseDoubleClick
        Try
            Dim dgv As DataGridView = DirectCast(sender, DataGridView)
            Dim hti As DataGridView.HitTestInfo = dgv.HitTest(e.X, e.Y)

            'マウスの位置がセル以外(列ヘッダ)の場合は終了
            If hti.Type <> DataGridViewHitTestType.Cell Then
                Exit Sub
            End If

            '1行以上明細が存在する　And 選択中の行が存在する　And 念のため
            If (dgv.RowCount > 0) AndAlso (dgv.CurrentRow IsNot Nothing) AndAlso (dgv.CurrentRow.Index > -1) Then
                btnF7.PerformClick()
            End If
        Catch ex As Exception
            ErrorMessage(ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' F7:詳細表示(エンター)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DGV_Detail_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles DGV.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            Dim dgv As DataGridView = DirectCast(sender, DataGridView)
            '1行以上明細が存在する AND 選択中の行が存在する　AND 念のため
            If (dgv.RowCount > 0) AndAlso (dgv.CurrentRow IsNot Nothing) AndAlso (dgv.CurrentRow.Index > -1) Then
                btnF7.PerformClick()
            End If
        End If
    End Sub

    ''' <summary>
    ''' 実行(F9)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF9_Click(sender As Object, e As EventArgs)
        If (CheckItem(True) = False) Then
            Return
        End If
        '検索処理
        GamenHyojiSyori(True, True)
    End Sub

    ''' <summary>
    ''' 終了(F12)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF12_Click(sender As Object, e As EventArgs)
        If DGV.RowCount > 0 Then
            '一覧表示中は取り消し
            InitControl(False)
            SelectNextControl(Me, True, True, True, True)   '初期フォーカスのセット
        Else
            MyBase.BtnF12_Click(sender, e)
        End If
    End Sub
#End Region

    ''' <summary>
    ''' 出荷場所、取引先、部品番号の値が入力されたとき
    ''' </summary>
    ''' <param name="target"></param>
    ''' <param name="e"></param>
    Private Sub SearchCondition_SettingControlValue(target As Control, e As System.ComponentModel.HandledEventArgs) Handles T_ToriCD.SettingControlValue, T_BuhinNo.SettingControlValue, C_ShukkaBasho.SettingControlValue

        'GamenHyojiSyori(False, False)

    End Sub

    ''' <summary>
    ''' 納入予定日(終了)の値が入力されたとき
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub D_Yotei_SettingControlValue(sender As System.Object, e As System.EventArgs) Handles D_YoteiEd.SettingControlValue

        Dim strWk(1) As String

        '大小チェック
        If checkDateTerm(D_YoteiSt, D_YoteiEd, "納入予定日", strWk) = False Then Return

        '検索結果表示
        'GamenHyojiSyori(False, False)
    End Sub

    ''' <summary>
    ''' 納入指示日(終了)の値が入力されたとき
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub D_Siji_SettingControlValue(sender As System.Object, e As System.EventArgs) Handles D_SijiEd.SettingControlValue

        Dim strWk(1) As String

        '大小チェック
        If checkDateTerm(D_SijiSt, D_SijiEd, "納入指示日", strWk) = False Then Return

        '検索結果表示
        'GamenHyojiSyori(False, False)
    End Sub

    ''' <summary>
    ''' 条件入力チェック
    ''' </summary>
    ''' <param name="blnMsg">エラーメッセージを表示するときはTrue</param>
    ''' <returns></returns>
    Private Function checkConditon(ByVal blnMsg As Boolean) As Boolean

        Dim strYotei(1) As String
        Dim strSiji(1) As String

        '納入予定日　開始終了チェック
        If checkDateTerm(D_YoteiSt, D_YoteiEd, "納入予定日", strYotei) = False Then
            Return False
        End If

        '納入指示日　開始終了チェック
        If checkDateTerm(D_SijiSt, D_SijiEd, "納入指示日", strSiji) = False Then
            Return False
        End If

        If strYotei(0) = "0" AndAlso strYotei(1) = "0" AndAlso strSiji(0) = "0" AndAlso strSiji(1) = "0" _
        AndAlso T_ToriCD.Text = "" AndAlso T_BuhinNo.Text = "" Then
            If blnMsg = True Then
                WarnMessage("納入予定日～部品番号のいずれか1つは条件を指定してください")
                ActiveControl = FocusedControl
            End If
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' 検索結果表示
    ''' </summary>
    ''' <param name="isButton"></param>
    ''' <param name="isForce"></param>
    Private Sub GamenHyojiSyori(isButton As Boolean, isForce As Boolean)

        '前回検索時と検索条件が同じ場合検索しない
        Dim newSearchConditon As New Dictionary(Of String, Object) From {
            {C_ShukkaBasho.Name, C_ShukkaBasho.SelectedValue.ToString},
            {D_YoteiSt.Name, D_YoteiSt.ValueRef},
            {D_YoteiEd.Name, D_YoteiEd.ValueRef},
            {D_SijiSt.Name, D_SijiSt.ValueRef},
            {D_SijiEd.Name, D_SijiEd.ValueRef},
            {T_ToriCD.Name, T_ToriCD.ValueRef},
            {T_BuhinNo.Name, T_BuhinNo.ValueRef}}

        If (SearchCondition.SequenceEqual(newSearchConditon)) Then
            If isForce = False Then
                Return
            End If
        End If

        '検索条件を保存
        SearchCondition = newSearchConditon

        '条件の入力チェック
        If checkConditon(isButton) = False Then Return

        If (CheckItem(True) = False) Then
            Return
        End If

        '2019/04/20 by常門
        Me.Cursor = Cursors.WaitCursor

        DGV.DataSource = Nothing
        btnF7.Enabled = False

        '検索
        Dim yoteiDtSt As Date = If(D_YoteiSt.ValueIsNull, Date.MinValue, D_YoteiSt.Value)
        Dim yoteiDtEd As Date = If(D_YoteiEd.ValueIsNull, Date.MinValue, D_YoteiEd.Value)
        Dim sijiDtSt As Date = If(D_SijiSt.ValueIsNull, Date.MinValue, D_SijiSt.Value)
        Dim sijiDtEd As Date = If(D_SijiEd.ValueIsNull, Date.MinValue, D_SijiEd.Value)

        Dim dt As DataTable = Logic.GetShipResult(Ibr, C_ShukkaBasho.SelectedValue.ToString, yoteiDtSt, yoteiDtEd, sijiDtSt, sijiDtEd, T_ToriCD.Text, T_BuhinNo.Text, C_ShukkaBasho.Text)
        If ((dt Is Nothing) OrElse dt.Rows.Count = 0) AndAlso isForce = True Then
            WarnMessage("該当データがありません")
            D_YoteiSt.Focus()
            '2019/04/20 by常門
            Me.Cursor = Cursors.Default
            Return
        Else
            'If dt.Rows.Count > 2000 Then
            'WarnMessage("明細件数が2000件を超えています")
            'End If
            DGV.DataSource = dt
        End If

        btnF7.Enabled = (dt.Rows.Count > 0)

        If dt.Rows.Count > 0 Then
            DGV.CurrentCell = DGV(0, 0)
        End If
        DGV.Focus()
        '2019/04/20 by常門
        Me.Cursor = Cursors.Default

    End Sub

    ''' <summary>
    ''' 日付の大小チェック
    ''' </summary>
    ''' <param name="dtSt">日付開始のコントロール</param>
    ''' <param name="dtEd">日付終了のコントロール</param>
    ''' <param name="strNM">項目名</param>
    ''' <param name="strWk">入力された日付</param>
    ''' <returns></returns>
    Private Function checkDateTerm(ByVal dtSt As ComDateSjk, ByVal dtEd As ComDateSjk, ByVal strNM As String, ByRef strWk() As String) As Boolean

        '大小チェック
        ReDim strWk(1)
        For i As Integer = 0 To 1
            If i = 0 Then
                strWk(i) = dtSt.Text
            Else
                strWk(i) = dtEd.Text
            End If
            If strWk(i) = "____/__/__" Then
                strWk(i) = "0"
            Else
                strWk(i) = strWk(i).Replace("/", "")
            End If
            If i = 1 AndAlso strWk(i) = "0" Then
                strWk(i) = "99999999"
            End If
        Next

        If strWk(0) > strWk(1) Then
            WarnMessage(strNM & "が正しくありません")
            dtSt.Focus()
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' 一覧のダブルクリックイベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ComDataGridViewSjk1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles DGV.MouseDoubleClick
        Try
            Dim hti As DataGridView.HitTestInfo = DirectCast(sender, DataGridView).HitTest(e.X, e.Y)
            '▼マウスの位置が列ヘッダの場合は終了
            If hti.Type <> DataGridViewHitTestType.Cell Then
                Exit Sub
            End If
            DialogResult = DialogResult.OK
        Catch ex As Exception
            IbrCom.Message(Err.Description, , MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' 一覧のKeyDownイベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ComDataGridViewSjk1_KeyDown(sender As Object, e As KeyEventArgs) Handles DGV.KeyDown
        If (e.KeyData = Keys.Enter) Then
            If (DGV.CurrentRow Is Nothing) OrElse (DGV.CurrentRow.Index < 0) Then
                Return
            End If
            DialogResult = DialogResult.OK
        End If
    End Sub

End Class
