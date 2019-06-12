Imports System.Text
Imports IBR_SKS_C001
Imports Oracle.DataAccess.Client

''' <summary>
''' データ取得IBR_SJK_S030, IBR_SJK_S040共通ロジック
''' </summary>
Friend Class SyukkaRef_001

    ''' <summary>
    ''' 検索条件より出荷実績を検索する
    ''' </summary>
    ''' <param name="ibr">接続情報</param>
    ''' <param name="shukkaBasho">出荷場所</param>
    ''' <param name="yoteiDtSt">納入予定日開始</param>
    ''' <param name="yoteiDtEd">納入予定日終了</param>
    ''' <param name="sijiDtSt">納入指示日開始</param>
    ''' <param name="sijiDtEd">納入指示日終了</param>
    ''' <param name="toriCd">取引先</param>
    ''' <param name="bhnNo">部品番号</param>
    ''' <param name="strShukkaBashoNM">コンボボックスの出荷場所の選択テキスト</param>
    ''' <returns>出荷実績テーブル検索結果</returns>
    ''' <remarks>存在しない場合はNothingを返却する</remarks>
    Public Shared Function GetShipResult(ByVal ibr As IbrData, ByVal shukkaBasho As String, ByVal yoteiDtSt As Date, ByVal yoteiDtEd As Date, ByVal sijiDtSt As Date, ByVal sijiDtEd As Date, ByVal toriCd As String, ByVal bhnNo As String, ByVal strShukkaBashoNM As String) As DataTable

        Dim dt As New DataTable
        Dim strOrder As String = " ORDER BY E.DELIVERY_IND_DATE, E.DELIVERY_EST_DATE, E.CLIENT_CD, R.TICKET_PARTS_NO, E.ORDER_NO, R.SCAN_DATE, R.SCAN_TIME"
        ibr.Cmd.BindByName = True
        ibr.Cmd.Parameters.Clear()
        Dim sb As New StringBuilder

        sb.AppendLine("SELECT ROW_NUMBER() OVER (" & strOrder & ") ROW_NO") '行番号
        sb.AppendLine("     , R.SCAN_DATE SHUKKA_DT")           '出荷日
        sb.AppendLine("     , E.DELIVERY_IND_DATE SIJI_DT")     '納入指示日
        sb.AppendLine("     , E.DELIVERY_EST_DATE YOTEI_DT")    '納入予定日
        sb.AppendLine("     , Trim(T.TOK_NM_RYAK) TORIHIKI")    '取引先
        sb.AppendLine("     , E.BIN_CD BIN_CD")                 '便コード
        sb.AppendLine("     , R.TICKET_PARTS_NO BHN_NO")        '部品番号
        sb.AppendLine("     , E.DELIVERY_NO NOHINSHO_NO")       '納品書番号
        sb.AppendLine("     , E.BRANCH_NO EDA_NO")              '枝番
        sb.AppendLine("     , E.AMOUNT SIJISU")                 '指示数
        sb.AppendLine("     , R.AMOUNT SHUKKASU")               '出荷数
        sb.AppendLine("     , (CASE WHEN (R.GAZOU_FILE Is NULL Or R.GAZOU_FILE = '') THEN '無' ELSE '有' END) GAZOU")  '画像
        sb.AppendLine("     , E.ORDER_NO JUTYUNO")              '自社受注番号
        sb.AppendLine("     , R.BACK_NO BACKNO")                '背番号
        sb.AppendLine("     , R.NOUNYU_CLS NOUNYU_KEITAI")      '納入形態
        sb.AppendLine("     , R.TOGETHER_MAKER TMAKER")         '集約メーカー
        sb.AppendLine("     , R.BRANCH_NO BRANCHNO")            '枝番
        sb.AppendLine("     , P.PSW02 TANTO_NM")                '担当者名
        sb.AppendLine("     , R.PUBLISH_FLG PUB_FLG")           '発行済フラグ
        sb.AppendLine("     , R.TRANS_FLG TRANS_FLG")           '転送済フラグ
        sb.AppendLine("     , R.GAZOU_FILE GAZOUFILE")          '画像ファイル
        sb.AppendLine("     , E.CLIENT_CD TORICD")              '取引先コード
        sb.AppendLine("     , R.SCAN_TIME SCANTIME")            '読込時刻
        sb.AppendLine("     , '" & strShukkaBashoNM & "' AS SHUKKABASHO")   '出荷場所

        sb.AppendLine(" FROM SHIPPING_ESTIMATE E INNER JOIN SHIPPING_RESULT R ON TRIM(E.ORDER_NO) = TRIM(R.ORDER_NO)")  '出荷予定、出荷実績テーブル
        sb.AppendLine(" LEFT JOIN MTOKUI T ON TRIM(E.CLIENT_CD) = TRIM(T.TOK_CD) ")         '得意先
        sb.AppendLine(" LEFT JOIN PASSWD P ON R.HT_CHARGE_CD = P.PSW01")

        sb.AppendLine(" WHERE 1 = 1")
        sb.AppendLine(" AND E.FACTORY_CD = :FACTORY_CD")        '出荷場所
        ibr.Cmd.Parameters.Add(New OracleParameter("FACTORY_CD", OracleDbType.Varchar2, shukkaBasho.Trim, ParameterDirection.Input))

        If (yoteiDtSt > Date.MinValue) Then
            sb.AppendLine(" AND E.DELIVERY_EST_DATE >= :DELIVERY_EST_DATEST")   '納入予定日開始
            ibr.Cmd.Parameters.Add(New OracleParameter("DELIVERY_EST_DATEST", OracleDbType.Date, CInt(DateUtil.ToInt(yoteiDtSt).ToString.PadLeft(8, "0"c).Substring(2)).ToString("00/00/00"), ParameterDirection.Input))
        End If

        If (yoteiDtEd > Date.MinValue) Then
            sb.AppendLine(" AND E.DELIVERY_EST_DATE <= :DELIVERY_EST_DATEED")       '納入予定日終了
            ibr.Cmd.Parameters.Add(New OracleParameter("DELIVERY_EST_DATEED", OracleDbType.Date, CInt(DateUtil.ToInt(yoteiDtEd).ToString.PadLeft(8, "0"c).Substring(2)).ToString("00/00/00"), ParameterDirection.Input))
        End If

        If (sijiDtSt > Date.MinValue) Then
            sb.AppendLine(" AND E.DELIVERY_IND_DATE >= :DERIVERY_IND_DATEST")         '納入指示日開始
            ibr.Cmd.Parameters.Add(New OracleParameter("DERIVERY_IND_DATEST", OracleDbType.Date, CInt(DateUtil.ToInt(sijiDtSt).ToString.PadLeft(8, "0"c).Substring(2)).ToString("00/00/00"), ParameterDirection.Input))
        End If

        If (sijiDtEd > Date.MinValue) Then
            sb.AppendLine(" AND E.DELIVERY_IND_DATE <= :DERIVERY_IND_DATEED")         '納入指示日終了
            ibr.Cmd.Parameters.Add(New OracleParameter("DERIVERY_IND_DATEED", OracleDbType.Date, CInt(DateUtil.ToInt(sijiDtEd).ToString.PadLeft(8, "0"c).Substring(2)).ToString("00/00/00"), ParameterDirection.Input))
        End If
        If (String.IsNullOrEmpty(bhnNo) = False) Then
            sb.AppendLine("AND TRIM(R.TICKET_PARTS_NO) = :TICKET_PARTS_NO")         '部品番号
            ibr.Cmd.Parameters.Add(New OracleParameter("TICKET_PARTS_NO", OracleDbType.Varchar2, bhnNo, ParameterDirection.Input))
        End If

        If (String.IsNullOrEmpty(toriCd) = False) Then
            sb.AppendLine("AND TRIM(E.CLIENT_CD) = :CLIENT_CD")                     '取引先
            ibr.Cmd.Parameters.Add(New OracleParameter("CLIENT_CD", OracleDbType.Varchar2, toriCd, ParameterDirection.Input))
        End If
        sb.AppendLine("AND R.ERROR_FLG = 0 ")                                       'エラー情報は除く

        sb.AppendLine(strOrder) '納入指示日、納入予定日、取引先コード、部品番号、自社受注番号、読込日,読込時刻順

        Try
            Using dr As OracleDataReader = ibr.DataReader(sb.ToString)
                If dr.HasRows = True Then
                    'dr.Read()
                    dt.Load(dr)
                End If
                Return dt
            End Using
        Catch ex As Exception
            Throw New ApplicationException(Messages.ErrSelect("出荷実績", ex), ex)
        End Try
    End Function

    ''' <summary>
    ''' 環境テーブルから画像ファイルの保存場所を取得
    ''' </summary>
    ''' <param name="key"></param>
    ''' <param name="ibr"></param>
    ''' <returns></returns>
    Public Shared Function getEnvValue(key As String, ibr As IbrData) As String
        Dim ret As String = String.Empty
        Try
            ibr.Cmd.BindByName = True
            ibr.Cmd.Parameters.Clear()
            ibr.Cmd.Parameters.Add(New OracleParameter("KEY", OracleDbType.Char, key, ParameterDirection.Input))

            Using dr As OracleDataReader = ibr.DataReader("SELECt VALUE1 FROM MENVIR WHERE KEY =:KEY")
                If (dr.Read()) Then
                    ret = dr.GetString(0).TrimEnd
                End If
            End Using

            Return ret
        Catch ex As Exception
            IbrCom.Message(Err.Description, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return ret
        End Try
    End Function
End Class
