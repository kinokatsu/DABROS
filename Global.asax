<%@ Application Language="VB" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' アプリケーションのスタートアップで実行するコードです
        RegisterRoutes(RouteTable.Routes)
    End Sub
    
    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' アプリケーションのシャットダウンで実行するコードです
    End Sub
        
    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' ハンドルされていないエラーが発生したときに実行するコードです
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' 新規セッションを開始したときに実行するコードです
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' セッションが終了したときに実行するコードです 
        ' メモ: Session_End イベントは、Web.config ファイル内で sessionstate モードが
        ' InProc に設定されているときのみ発生します。session モードが StateServer か、または 
        ' SQLServer に設定されている場合、イベントは発生しません。
    End Sub

    Sub RegisterRoutes(ByVal routes As RouteCollection)
        
        routes.Add("BikeSaleRoute", New Route("{LoggerID}/WaterLevel", New CustomRouteHandler("~/Graph_WLTimeChart.aspx")))
        
        'routes.MapPageRoute("", "WaterLevel/{LoggerID}", "~/Graph_WLTimeChart.aspx")
        'routes.MapPageRoute("", "{LoggerID}/WaterLevel", "~/Graph_WLTimeChart.aspx")
    
        'このコードは、SalesRoute という名前のルートを追加します。 このルートに名前を付ける理由は、このルートが次のステップで作成するルートと同じパラメーター リストを持っているためです。 2 つのルートに名前を割り当てることで、それらの URL を生成するときに両者を区別できます。
        routes.MapPageRoute("SalesRoute", "SalesReport/{locale}/{year}", "~/sales.aspx")
        
        'このコードは、ExpensesRoute という名前のルートを追加します。 このルートには、extrainfo という汎用的なパラメーターが含まれます。 このコードでは、locale パラメーターの既定値を "US" に設定し、year パラメーターの既定値を現在の年に設定します。 制約として、locale パラメーターが英字 2 文字で構成されている必要があること、および year パラメーターが 4 桁の数字で構成されている必要があることを指定します。
        routes.MapPageRoute("ExpensesRoute", "ExpenseReport/{locale}/{year}/{*extrainfo}", "~/expenses.aspx", True,
                            New RouteValueDictionary(New With {.locale = "US", .year = DateTime.Now.Year.ToString()}),
                            New RouteValueDictionary(New With {.locale = "[a-z]{2}", .year = "\d{4}"}))
        
    End Sub

</script>

