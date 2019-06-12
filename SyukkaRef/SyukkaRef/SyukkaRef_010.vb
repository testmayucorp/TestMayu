Imports System.Text
Imports Logic = SyukkaRef.SyukkaRef_001
Imports Oracle.DataAccess.Client

Public Class SyukkaRef_010
    Inherits IbrFormBase

    Private frm000 As SyukkaRef_000
    Private marrGazou As ArrayList
    Private mintGazouInd As Integer     '現在表示中の画像のインデックス

    Public Sub New(ByVal ibr As IbrData, ByVal frm As SyukkaRef_000, ByVal strShukkaBasho As String)
        MyBase.New(ibr)
        Me.frm000 = frm
        Me.marrGazou = New ArrayList

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。

    End Sub

    ''' <summary>
    ''' 前の画像(F3)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF3_Click(sender As Object, e As EventArgs)

        If Me.mintGazouInd > 0 Then
            Me.mintGazouInd -= 1 'ひとつ前の画像を表示
            dispGazou()
        End If
    End Sub

    ''' <summary>
    ''' 次の画像(F4)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF4_Click(sender As Object, e As EventArgs)

        If Me.mintGazouInd < Me.marrGazou.Count - 1 Then
            Me.mintGazouInd += 1 'ひとつ次の画像を表示
            dispGazou()
        End If
    End Sub

    ''' <summary>
    ''' 前項目(F5)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF5_Click(sender As Object, e As EventArgs)
        Get_back_record()
    End Sub

    ''' <summary>
    ''' 次項目(F8)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF8_Click(sender As Object, e As EventArgs)
        Get_next_record()
    End Sub

    ''' <summary>
    ''' 画像保存(F9)ボタン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Overrides Sub BtnF9_Click(sender As Object, e As EventArgs)

        SaveFileDialog1.Filter = "GIF形式|*.gif|JPEG形式|*.jpeg|PNG形式|*.png"
        SaveFileDialog1.FilterIndex = 2
        SaveFileDialog1.FileName = T_Gazou.Text

        '画像の保存ダイアログを表示
        SaveFileDialog1.ShowDialog()

    End Sub

    ''' <summary>
    ''' 一つ前の明細を表示
    ''' </summary>
    Private Sub Get_back_record()
        If frm000.dgvCount = 0 Then
            MessageBox.Show("前データは存在しません。", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        frm000.dgvCount -= 1
        TextInput()
    End Sub

    ''' <summary>
    ''' 一つ次の項目を表示
    ''' </summary>
    Private Sub Get_next_record()
        If frm000.dgvCount = frm000.DGV.Rows.Count - 1 Then
            MessageBox.Show("次データは存在しません。", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        frm000.dgvCount += 1
        TextInput()
    End Sub

    ''' <summary>
    ''' データ表示
    ''' </summary>
    Private Sub TextInput()

        Try
            Dim intRowNo As Integer
            intRowNo = CInt(frm000.DGV.Rows(frm000.dgvCount).Cells("columnRowNo").Value)

            Dim dt As DataRow() = DirectCast(frm000.DGV.DataSource, DataTable).Select("ROW_NO = " & intRowNo.ToString)
            If dt.Length = 0 Then   'ありえないが
                MessageBox.Show("選択された行の詳細情報が存在しません。", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            D_SyukkaDt.Text = DataUtil.NvlStr(dt(0)("SHUKKA_DT"))                    '出荷日
            D_YoteiDt.Text = DataUtil.NvlStr(dt(0)("YOTEI_DT"))                      '納入予定日              
            D_SijiDt.Text = DataUtil.NvlStr(dt(0)("SIJI_DT"))                        '納入指示日
            T_ShukkaBasho.Text = DataUtil.NvlStr(dt(0)("SHUKKABASHO"))               '出荷場所
            T_ToriCD.Text = DataUtil.NvlStr(dt(0)("TORICD"))                         '取引先コード
            T_ToriNM.Text = DataUtil.NvlStr(dt(0)("TORIHIKI"))                       '取引先名
            T_NohinNo.Text = DataUtil.NvlStr(dt(0)("NOHINSHO_NO"))                   '納品書番号
            T_EdaNo.Text = DataUtil.NvlStr(dt(0)("EDA_NO"))                          '枝番
            T_SijiSu.Text = DataUtil.NvlInt(dt(0)("SIJISU")).ToString("#,##0")       '指示数
            T_SyukkaSu.Text = DataUtil.NvlInt(dt(0)("SHUKKASU")).ToString("#,##0")   '出荷数
            T_Maker.Text = DataUtil.NvlStr(dt(0)("TMAKER"))                          '集約メーカー
            T_Keitai.Text = DataUtil.NvlStr(dt(0)("NOUNYU_KEITAI"))                  '納入形態
            T_BackNo.Text = DataUtil.NvlStr(dt(0)("BACKNO"))                         '背番号
            T_JutyuNo.Text = DataUtil.NvlStr(dt(0)("JUTYUNO"))                       '自社受注番号
            T_BinCd.Text = DataUtil.NvlStr(dt(0)("BIN_CD"))                          '便コード
            T_BuhinNo.Text = DataUtil.NvlStr(dt(0)("BHN_NO"))                        '部品番号
            T_BuhinNM.Text = getBuhinNM(DataUtil.NvlStr(dt(0)("BHN_NO")), DataUtil.NvlStr(dt(0)("SHUKKA_DT")))    '部品名
            D_ReadDt.Text = DataUtil.NvlStr(dt(0)("SHUKKA_DT"))                      '読込日
            T_ReadTm.Text = CDate(dt(0)("SCANTIME")).ToString("HH:mm:ss")            '読込時刻
            T_Tanto.Text = DataUtil.NvlStr(dt(0)("TANTO_NM"))                        '担当者
            If DataUtil.NvlStr(dt(0)("PUB_FLG")) = "1" Then                          'リスト発行
                T_ListHakko.Text = "済"
            Else
                T_ListHakko.Text = "未"
            End If
            If DataUtil.NvlStr(dt(0)("TRANS_FLG")) = "1" Then                        'オフコン転送
                T_OfconTensou.Text = "済"
            Else
                T_OfconTensou.Text = "未"
            End If
            '画像の表示
            Me.marrGazou.Clear()
            Me.mintGazouInd = -1
            lblPage.Text = ""
            T_Gazou.Text = "画像なし"
            PctGazou.ImageLocation = ""
            btnF3.Enabled = False
            btnF4.Enabled = False
            btnF9.Enabled = False
            If DataUtil.NvlStr(dt(0)("GAZOUFILE")) IsNot Nothing AndAlso DataUtil.NvlStr(dt(0)("GAZOUFILE")).Trim <> "" Then
                Me.marrGazou = getGazouFiles(DataUtil.NvlStr(dt(0)("BHN_NO")), DataUtil.NvlStr(dt(0)("SHUKKA_DT")), DataUtil.NvlStr(dt(0)("SCANTIME")), DataUtil.NvlStr(dt(0)("JUTYUNO")))
                If Me.marrGazou.Count > 0 Then
                    Me.mintGazouInd = 0 '撮影順で一番最初に撮影したものを表示
                    '画像の表示
                    dispGazou()
                End If
            End If
        Catch ex As Exception
            ErrorMessage(ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' 部品名取得
    ''' </summary>
    ''' <param name="strBuhinNo">部品番号</param>
    ''' <param name="strSyukkaDt">出荷日</param>
    ''' <returns></returns>
    Private Function getBuhinNM(ByVal strBuhinNo As String, ByVal strSyukkaDt As String) As String

        Dim sb As New StringBuilder

        sb.AppendLine(" SELECT BHN_NM")
        sb.AppendLine(" FROM MBUHIN")
        sb.AppendLine(" WHERE BHN_NO =:BHN_NO")
        sb.AppendLine(" AND YUKO_DT_FROM <= :SYUKKA_DT")
        sb.AppendLine(" AND YUKO_DT_TO >= :SYUKKA_DT")
        Ibr.Cmd.BindByName = True
        Ibr.Cmd.Parameters.Clear()
        Ibr.Cmd.Parameters.Add(New OracleParameter("BHN_NO", OracleDbType.Char, strBuhinNo, ParameterDirection.Input))
        Ibr.Cmd.Parameters.Add(New OracleParameter("SYUKKA_DT", OracleDbType.Int32, DataUtil.NvlInt(CDate(strSyukkaDt).ToString("yyyyMMdd")), ParameterDirection.Input))
        Try
            Using dr As OracleDataReader = Ibr.DataReader(sb.ToString)
                If dr.HasRows = True Then
                    dr.Read()
                    Return DataUtil.NvlStr(dr.Item("BHN_NM"))
                End If
            End Using
        Catch ex As Exception
            Throw New ApplicationException(Messages.ErrSelect("部品マスタ", ex), ex)
            Return ""
        End Try

        Return ""
    End Function

    ''' <summary>
    ''' 全コントロールの初期化処理
    ''' </summary>
    ''' <param name="onFormLoad">Form Load時の呼び出し</param>
    Public Overrides Sub InitControl(onFormLoad As Boolean)
        MyBase.InitControl(onFormLoad)

        'データ表示
        TextInput()

        '画面表示するだけのため終了ボタンにフォーカスを当てておく
        BeginInvoke(Sub() Me.ActiveControl = Me.btnF12)
    End Sub

    ''' <summary>
    ''' 画像の表示
    ''' </summary>
    Private Sub dispGazou()

        Try
            '画像ファイル名
            T_Gazou.Text = System.IO.Path.GetFileName(Me.marrGazou(Me.mintGazouInd).ToString)
            '画像表示
            Me.PctGazou.ImageLocation = Me.marrGazou(Me.mintGazouInd).ToString

            '表示中のページ
            lblPage.Text = (Me.mintGazouInd + 1) & "/" & Me.marrGazou.Count.ToString

            btnF3.Enabled = False
            btnF4.Enabled = False
            If Me.mintGazouInd > 0 Then
                btnF3.Enabled = True       '前の画像ボタン
            End If
            If Me.mintGazouInd < Me.marrGazou.Count - 1 Then
                btnF4.Enabled = True       '次の画像ボタン
            End If
            btnF9.Enabled = True            '画像保存ボタン

        Catch ex As Exception
            ErrorMessage(ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' 画像ファイルを取得
    ''' </summary>
    ''' <param name="strBuhinNo">部品番号</param>
    ''' <param name="strReadDate">読込日</param>
    ''' <param name="strReadtime">読込時刻</param>
    ''' <param name="strJutyuNo">自社受注番号</param>
    ''' <returns></returns>
    Private Function getGazouFiles(ByVal strBuhinNo As String, ByVal strReadDate As String, ByVal strReadtime As String, ByVal strJutyuNo As String) As ArrayList

        Dim arrWk As New ArrayList


        '画像ファイル保存パスにある画像を取得
        Dim wk As String() = System.IO.Directory.GetFiles(frm000.gazouPath, strBuhinNo & "_" & CDate(strReadDate).ToString("yyyyMMdd") & "_" & CDate(strReadtime).ToString("HHmmss") & "_" & strJutyuNo & "_*.JPEG")

        '撮影順に並び替え
        arrWk.Clear()
        For i As Integer = 0 To wk.Length - 1
            arrWk.Add(wk(i))
        Next
        arrWk.Sort()

        Return arrWk
    End Function

    ''' <summary>
    ''' 指定された形式で画像ファイルを保存する
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub SaveFileDialog1_FileOk(sender As Object, e As CancelEventArgs) Handles SaveFileDialog1.FileOk

        Dim extension As String = IO.Path.GetExtension(SaveFileDialog1.FileName)
        Select Case extension.ToUpper
            Case ".GIF"
                PctGazou.Image.Save(SaveFileDialog1.FileName, Imaging.ImageFormat.Gif)
            Case ".JPEG"
                PctGazou.Image.Save(SaveFileDialog1.FileName, Imaging.ImageFormat.Jpeg)
            Case ".PNG"
                PctGazou.Image.Save(SaveFileDialog1.FileName, Imaging.ImageFormat.Png)
        End Select
    End Sub
End Class
