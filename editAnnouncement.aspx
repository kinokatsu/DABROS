<%@ Page Language="VB" AutoEventWireup="false" CodeFile="editAnnouncement.aspx.vb" Inherits="editAnnouncement" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="C1.Web.UI.Controls.3" Namespace="C1.Web.UI.Controls.C1Calendar" TagPrefix="cc1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title>お知らせ編集</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <link href="CSS/stl_sexybuttons.css" rel="stylesheet" type="text/css" />
    <link href="CSS/stl_calendar.css" rel="stylesheet" type="text/css" />
    <link href="CSS/stl_startpage.css" rel="stylesheet" type="text/css" />
    <script src="js/jquery-3.0.0.min.js" type="text/javascript"></script>
    <script src="js/calendar.js" type="text/javascript"></script>
    <script src="js/tbl_Announce.js" type="text/javascript"></script>
</head>
<body style="text-align: center">
    <form id="SiteSelectFrm" runat="server">
        <div style="text-align: center; font-size: 11pt;" id="FrmSiteSelect">
            <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server" EnableScriptGlobalization="True" EnableScriptLocalization="True">
            </asp:ToolkitScriptManager>
            <asp:Panel ID="Pnl01" runat="server" BorderStyle="None" HorizontalAlign="Center">
                <table id="TABLE1" runat="server" style="width: 500px; text-align: center; margin-right: auto; margin-left: auto;border-width:0px;">
                    <tr>
                        <td style="color: white; height: 25px; font-weight: bold; font-size: 12pt; vertical-align: middle; text-align: center; width: 859px; background-color: #0000ec;" class="headBlock">
                            お知らせ編集
                        </td>
                    </tr>
                    <tr style="font-size: 11pt; color: #000000;"  >
                        <td style="font-size: 12pt; vertical-align: top; height: 50px; text-align: center; width: 859px; margin-right: auto; margin-left: auto;">
                            <table id="Table2" 
                                style="border-style: solid; border-width: 1px; border-color:#CE6700; padding: 1px; width: 630px; height: 190px; text-align: left; background-color: #996600; border-spacing: 0px; margin-right: auto; margin-left: auto;" >
                                <tr>
                                    <td class="TABLEHEADER" colspan="2" style="font-weight: bold; font-size: 11pt; vertical-align: middle; height: 20px; background-color: #cccccc; text-align: center; width: 630px;border-width:1px;">
                                        「お知らせ」の追加設定</td>
                                </tr>
                                <tr>
                                    <td colspan="2" style="font-size: 10pt; vertical-align: top;height: 82px; background-color: #FFF8DC">
                                        <ul>
                                            <li>「お知らせ」に追記する情報を設定します。<br />
                                        現場名称、表示ユーザ、表示期限は、実際に表示しない管理上のデータです。<br />
                                        HTMLタグは使用できません。また改行はなるべく入れないで下さい。<br />
                                        一度入力したものを編集することはできませんので、変更する場合は新規に追加して、不要なものを削除してください。
                                            </li>
                                        </ul>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="1" style="font-size: 10pt; width: 100px; height: 25px; background-color: #cccccc; border-width:2px 2px 1px 0px; border-style:solid; border-color:#ce6700;">■現場名称</td>
                                    <td colspan="1" style="font-size: 10pt; height: 25px; background-color: #FFF8DC; border-width:2px 0px 1px 0px; border-style:solid; border-color:#ce6700;">
                                        <asp:DropDownList ID="DDLSiteName" runat="server" Width="505px"></asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="1" style="font-size: 10pt; width: 100px; height: 25px; background-color: #cccccc; border-width:1px 2px 1px 0px; border-style:solid; border-color:#ce6700;">■表示ユーザ</td>
                                    <td colspan="1" style="font-size: 11pt; background-color: #FFF8DC; border-width:1px 0px 1px 0px; border-style:solid; border-color:#ce6700;">
                                        <asp:CheckBoxList ID="ChkBListUser" runat="server" BorderStyle="None" RepeatDirection="Horizontal" Font-Size="9pt">
                                        </asp:CheckBoxList><asp:Panel ID="PnlUserNm" runat="server" BorderStyle="None"
                                            Height="80px" Width="523px">
                                            <asp:ListBox ID="LstUserNames" runat="server" Height="80px" SelectionMode="Multiple" Width="257px"></asp:ListBox>
                                            <asp:ListBox ID="LstGroupName" runat="server" Height="80px" SelectionMode="Multiple" Visible="False" Width="257px"></asp:ListBox></asp:Panel>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="1" style="font-size: 10pt; width: 100px; height: 25px; background-color: #cccccc; vertical-align: middle; border-width:1px 2px 1px 0px; border-style:solid; border-color:#ce6700;">■表示開始日</td>
                                    <td colspan="1" style="font-size: 10pt; height: 25px;background-color: #FFF8DC; vertical-align: middle; border-spacing: 0px;border-width:1px 0px 1px 0px; border-style:solid; border-color:#ce6700;">
                                        <asp:TextBox ID="TxtStDate" runat="server" CssClass="TEXTBOXES" EnableTheming="True" Font-Size="10pt" MaxLength="10" ToolTip="「指定日」の時に、データ表示開始日時を入力してください" ValidationGroup="DateCheck" Width="100px"></asp:TextBox>
                                        <asp:MaskedEditExtender ID="TxtStDate_MaskedEditExtender" runat="server" 
                                            CultureAMPMPlaceholder="" CultureCurrencySymbolPlaceholder="" 
                                            CultureDateFormat="" CultureDatePlaceholder="" CultureDecimalPlaceholder="" 
                                            CultureThousandsPlaceholder="" CultureTimePlaceholder="" Enabled="True" 
                                            TargetControlID="TxtStDate" AutoComplete="False" Mask="9999/99/99" 
                                            MaskType="Date">
                                        </asp:MaskedEditExtender>
                                        <asp:Image ID="imgCalTxtStDate" runat="server" EnableTheming="False" ImageUrl="~/img/Calendar.gif" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" CssClass="calBtn" />　
                                        <asp:DropDownList ID="DDListHourSt" runat="server" Width="48px"></asp:DropDownList>時
                                        <asp:DropDownList ID="DDListMinuteSt" runat="server" Width="48px"></asp:DropDownList>分
                                        <%--<div ID="NowDtBtn" class="clear" style="width: 59px; height: 20px;Position:Absolute;left:560px">--%>
                                        <a class="sexybutton sexysimple sexysmall" href="#" onclick="NowDateTime('TxtStDate');return false;" style="text-align: center;" >現在日時</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="1" style="font-size: 10pt; width: 100px; height: 25px; background-color: #cccccc; vertical-align: middle; border-width:1px 2px 1px 0px; border-style:solid; border-color:#ce6700;">■表示期限</td>
                                    <td colspan="1" style="font-size: 10pt; height: 25px;background-color: #FFF8DC; vertical-align: middle; border-spacing: 0px;border-width:1px 0px 1px 0px; border-style:solid; border-color:#ce6700;">
                                        <asp:TextBox ID="TxtLimitDate" runat="server" CssClass="TEXTBOXES" EnableTheming="True" Font-Size="10pt" MaxLength="10" ToolTip="「指定日」の時に、データ表示開始日時を入力してください" ValidationGroup="DateCheck" Width="100px"></asp:TextBox>
                                        <asp:MaskedEditExtender ID="TxtLimitDate_MaskedEditExtender" runat="server" 
                                            CultureAMPMPlaceholder="" CultureCurrencySymbolPlaceholder="" 
                                            CultureDateFormat="" CultureDatePlaceholder="" CultureDecimalPlaceholder="" 
                                            CultureThousandsPlaceholder="" CultureTimePlaceholder="" Enabled="True" 
                                            TargetControlID="TxtLimitDate" Mask="9999/99/99" MaskType="Date" UserDateFormat="YearMonthDay" AutoComplete="False">
                                        </asp:MaskedEditExtender>
                                        <asp:Image ID="imgCalTxtLimitDate" runat="server" EnableTheming="False" ImageUrl="~/img/Calendar.gif" ToolTip="「指定期間」の時に、ここをクリックするとカレンダーを表示します" ImageAlign="Middle" />　
                                        <asp:DropDownList ID="DDListHour" runat="server" Width="48px"></asp:DropDownList>時 
                                        <asp:DropDownList ID="DDListMinute" runat="server" Width="48px"></asp:DropDownList>分 
                                        <div style="font-size:10pt;display:inline">※初期は1年後の日付を表示しています</div>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="1" style="font-size: 10pt; width: 100px; height: 120px; background-color: #cccccc; border-width:1px 2px 1px 0px; border-style:solid; border-color:#ce6700;">
                                        ■内容</td>
                                    <td colspan="1" style="font-size: 11pt; height: 120px;background-color: #FFF8DC;border-width:1px 0px 1px 0px; border-style:solid; border-color:#ce6700;">
                                <asp:TextBox ID="txtContent" runat="server" Height="104px" MaxLength="256" TextMode="MultiLine"
                                    Width="512px" CausesValidation="True" CssClass="TEXTBOXCOMMENT" BorderStyle="Inset" placeholder="ここに内容を記載して下さい。" ></asp:TextBox></td>
                                </tr>
                                <tr>
                                    <td colspan="2" style="font-size: 11pt; vertical-align: middle;height: 36px; background-color: #FFF8DC; text-align: center; border-width:1px 0px 1px 0px; border-style:solid; border-color:#ce6700;">
                                        <asp:Button ID="BtnAddRecord" runat="server" Width="1px" Height="1px" EnableTheming="False" EnableViewState="False" BackColor="Cornsilk" BorderColor="Cornsilk" BorderStyle="None" BorderWidth="0px" />
                                        <button class="sexybutton sexymedium" onclick="__doPostBack('BtnAddRecord','');return false;"><span><span><span class="edit">追 加</span></span></span></button>
                                        <%--<button class="sexybutton sexymedium" onclick="btnAddClick();return false;"><span><span><span class="edit">追 加</span></span></span></button>--%></td>
                                </tr>
                            </table>
                            </td>
                    </tr>
                    <tr style="font-size: 11pt; color: #000000">
                        <td style="height: 50px; font-size: 12pt; vertical-align: top; text-align: left; width: 859px;" >
                            <asp:GridView ID="GrdView" runat="server" BackColor="White"
                                BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" DataSourceID="AcDatSrc"
                                ForeColor="Black" GridLines="Vertical" Width="856px" AllowPaging="True" AllowSorting="True" EmptyDataText="---" Font-Size="9pt" ToolTip="お知らせ" PageSize="15">
                                <RowStyle BackColor="#F7F7DE" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                                <Columns>
                                    <asp:TemplateField HeaderText="削除" ShowHeader="False">
                                        <ItemTemplate>
                                            <asp:Button ID="Button1" runat="server" CausesValidation="false" CommandName="RecordDelete"
                                                OnClientClick='return confirm("削除しますか？")' Text="削除" />
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                        <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                    </asp:TemplateField>
                                </Columns>
                                <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Center" VerticalAlign="Top" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                                <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1px" />
                                <AlternatingRowStyle BackColor="White" />
                                <PagerTemplate>
                                    &nbsp;<asp:ImageButton ID="ImgBtnFirst" runat="server" AlternateText="最初" ImageUrl="~/img/First.png" CommandArgument="First" CommandName="Page" ToolTip="最初のページへ" />
                                    <asp:ImageButton ID="ImgBtnPrev" runat="server" AlternateText="前" ImageUrl="~/img/prev.png" CommandArgument="Prev" CommandName="Page" ToolTip="前のページへ" />
                                    [ <%= GrdView.PageIndex+1 %> / <%= GrdView.PageCount %> ]
                                    <asp:ImageButton ID="ImgBtnNext" runat="server" AlternateText="次"  ImageUrl="~/img/Next.png" CommandArgument="Next" CommandName="Page" ToolTip="次のページへ" />
                                    <asp:ImageButton ID="ImgBtnLast" runat="server" AlternateText="最後" ImageUrl="~/img/Last.png" CommandArgument="Last" CommandName="Page" ToolTip="最後のページへ" />
                                </PagerTemplate>
                                <EditRowStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                <EmptyDataTemplate>
                                </EmptyDataTemplate><FooterStyle BackColor="#CCCC99" />
                            </asp:GridView>
                            <asp:AccessDataSource ID="AcDatSrc" runat="server" DeleteCommand="UPDATE お知らせ SET 削除 = [True]"></asp:AccessDataSource>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
        </div>
<%--        <wijmo:C1Calendar ID="C1WebCalendar1" runat="server" Culture="ja-JP" 
            TitleFormat="- yyyy年MMMM -">
        </wijmo:C1Calendar>--%>
        <cc1:C1Calendar ID="C1WebCalendar" runat="server" height="230px" width="248px" 
            CalendarTitle="－ {0:yyyy'年'M'月'} －" CultureInfo="ja-JP" 
            Easing="EaseOutQuart" 
            NavigationEffect="Auto" PopupMode="True" 
            UseEmbeddedVisualStyles="False" VisualStyle="OriginalCal" 
            VisualStylePath="~/VisualStyles" ClientOnAfterClose="CalendarClosed" 
            ToolTipDateFormat="yyyy/MM/dd" NextToolTip="1ヶ月先へ" PrevToolTip="1ヶ月前へ" 
            QuickNextToolTip="3ヶ月先へ" QuickPrevToolTip="3ヶ月前へ">
        </cc1:C1Calendar>	
        <script src="~/js/calendar_Wijmo.js" type="text/javascript"></script>
        <asp:RangeValidator ID="RngValidLt" runat="server" ControlToValidate="TxtLimitDate"
            Display="None" EnableTheming="True" ErrorMessage="表示最大期限は本日より1年間です。" MaximumValue="2050/12/31"
            MinimumValue="1990/01/01" SetFocusOnError="True" Type="Date"></asp:RangeValidator><asp:RegularExpressionValidator
                ID="RExValidLt" runat="server" ControlToValidate="TxtLimitDate" Display="None"
                EnableTheming="True" ErrorMessage="指定日は正しくありません。再入力してください。" SetFocusOnError="True"
                ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))"
                ValidationGroup="EndDay"></asp:RegularExpressionValidator><asp:RangeValidator ID="RngValidSt"
                    runat="server" ControlToValidate="TxtStDate" Display="None" EnableTheming="True"
                    ErrorMessage="表示最大期限は本日より1ヵ月間です。" MaximumValue="2050/12/31" MinimumValue="1990/01/01"
                    SetFocusOnError="True" Type="Date"></asp:RangeValidator><asp:RegularExpressionValidator
                        ID="RExValidSt" runat="server" ControlToValidate="TxtStDate" Display="None" EnableTheming="True"
                        ErrorMessage="指定日は正しくありません。再入力してください。" SetFocusOnError="True" ValidationExpression="(?!([02468][1235679]|[13579][01345789])00\/02\/29)(([0-9]{4}\/(01|03|05|07|08|10|12)\/(0[1-9]|[12][0-9]|3[01]))|([0-9]{4}\/(04|06|09|11)\/(0[1-9]|[12][0-9]|30))|([0-9]{4}\/02\/(0[1-9]|1[0-9]|2[0-8]))|([0-9]{2}(([02468])[048]|[13579][26])\/02\/29))"
                        ValidationGroup="EndDay"></asp:RegularExpressionValidator><asp:RequiredFieldValidator
                            ID="RqFldValidContent" runat="server" ControlToValidate="txtContent" Display="None"
                            ErrorMessage="表示する内容を記載してください。" SetFocusOnError="True"></asp:RequiredFieldValidator>
        <asp:validatorcalloutextender ID="ValidCoExLt" runat="server" HighlightCssClass="validatorCalloutHighlight"
            TargetControlID="RExValidLt" Width="250px">
        </asp:validatorcalloutextender>
        <asp:validatorcalloutextender ID="ValidCoExRngLt" runat="server" HighlightCssClass="validatorCalloutHighlight"
            TargetControlID="RngValidLt" Width="250px">
        </asp:validatorcalloutextender>
        <asp:validatorcalloutextender ID="ValidCoExSt" runat="server" HighlightCssClass="validatorCalloutHighlight"
            TargetControlID="RExValidSt" Width="250px">
        </asp:validatorcalloutextender>
        <asp:validatorcalloutextender ID="ValidCoExRngSt" runat="server" HighlightCssClass="validatorCalloutHighlight"
            TargetControlID="RngValidSt" Width="250px">
        </asp:validatorcalloutextender>
        <asp:validatorcalloutextender ID="ValidCoExContent" runat="server" HighlightCssClass="validatorCalloutHighlight"
            TargetControlID="RqFldValidContent" Width="250px">
        </asp:validatorcalloutextender>
        <asp:animationextender ID="AnimEx" runat="server" EnableViewState="False"
            TargetControlID="TxtStDate">
            <Animations>
                <OnClick>
                    <Sequence>
                    <EnableAction Enabled="False" />
                    <FadeOut AnimationTarget="Pnl01" Duration="1" Fps="20" maximumOpacity="1.0" minimumOpacity="0.2" />
                    <EnableAction Enabled="True" />
                    <FadeIn AnimationTarget="Pnl01" Duration="1" Fps="20" maximumOpacity="1.0" minimumOpacity="0.2" />
                    </Sequence>
                </OnClick>
            </Animations>
        </asp:animationextender>
    </form>
</body>
</html>
