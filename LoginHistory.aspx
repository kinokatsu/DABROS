<%@ Page Language="VB" AutoEventWireup="false" CodeFile="LoginHistory.aspx.vb" Inherits="LoginHistory" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title>アクセス履歴</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <meta charset="utf-8" />
    <link href="CSS/stl_basic.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div title="アクセス履歴" style="text-align: center">
            <span style="text-decoration: underline"><strong>アクセス履歴</strong></span><br />
            <br />
            <asp:LoginStatus ID="LoginStatus1" runat="server" LogoutPageUrl="~/Login.aspx" />　
            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" EnableViewState="False" PostBackUrl="~/Admin.aspx">管理者用ページへ戻る</asp:LinkButton><br />
            <br />
            <asp:GridView ID="GrdView" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None"
                BorderWidth="1px" CellPadding="4" DataKeyNames="No" DataSourceID="AccessDataSource1"
                EmptyDataText="---" Font-Size="9pt" ForeColor="Black"
                GridLines="Vertical" Height="24px" PageSize="30" ToolTip="ログイン履歴">
                <FooterStyle BackColor="#CCCC99" />
                <RowStyle BackColor="#F7F7DE" />
                <Columns>
                    <asp:BoundField DataField="No" HeaderText="No" InsertVisible="False" ReadOnly="True" SortExpression="No">
                        <ItemStyle Width="30px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="日時" HeaderText="日時" SortExpression="日時">
                        <ItemStyle Width="138px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ユーザ名" HeaderText="ユーザ名" SortExpression="ユーザ名">
                        <ItemStyle Width="80px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="顧客名" HeaderText="顧客名" SortExpression="顧客名">
                        <ItemStyle Width="250px" HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ステータス" HeaderText="ステータス" SortExpression="ステータス">
                        <ItemStyle Width="70px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="リモートアドレス" HeaderText="リモートアドレス" SortExpression="リモートアドレス">
                        <ItemStyle Width="100px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="リモートホスト" HeaderText="リモートホスト" SortExpression="リモートホスト">
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="プラットフォーム" HeaderText="Platform" SortExpression="プラットフォーム">
                        <ItemStyle Width="70px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ブラウザ" HeaderText="Browser" SortExpression="ブラウザ">
                        <ItemStyle Width="50px" />
                    </asp:BoundField>
                </Columns>
                <PagerTemplate>
                    <asp:ImageButton ID="ImgBtnFirst" runat="server" AlternateText="最初" CommandArgument="First" CommandName="Page" ImageUrl="~/img/First.png" ToolTip="最初のページへ" CausesValidation="False" />
                    <asp:ImageButton ID="ImgBtnPrev" runat="server" AlternateText="前" CommandArgument="Prev" CommandName="Page" ImageUrl="~/img/prev.png" ToolTip="前のページへ" CausesValidation="False" />
                    <span style="vertical-align:super;">　
                    [
                    <%= GrdView.PageIndex+1 %>
                    /
                    <%= GrdView.PageCount %>
                    ] 　</span>
                    <asp:ImageButton ID="ImgBtnNext" runat="server" AlternateText="次" CommandArgument="Next" CommandName="Page" ImageUrl="~/img/Next.png" ToolTip="次のページへ" CausesValidation="False" />
                    <asp:ImageButton ID="ImgBtnLast" runat="server" AlternateText="最後" CommandArgument="Last" CommandName="Page" ImageUrl="~/img/Last.png" ToolTip="最後のページへ" CausesValidation="False" />
                </PagerTemplate>
                <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Center" VerticalAlign="Top" />
                <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
                <AlternatingRowStyle BackColor="White" />
            </asp:GridView>
            <asp:AccessDataSource ID="AccessDataSource1" runat="server" DataFile="C:\chkdb\AccessLog.mdb"
                SelectCommand="SELECT [No], [日時], [ユーザ名], [顧客名], [ステータス], [リモートアドレス], [リモートホスト], [プラットフォーム], [ブラウザ] FROM [アクセスログ] ORDER BY [No] DESC">
            </asp:AccessDataSource>
        </div>
    </form>
</body>
</html>
