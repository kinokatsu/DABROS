<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Question.ascx.vb" Inherits="Question" %>
    <div id="Item" class="Items" runat="server">
        <div id="title" class="Title" runat="server"></div>
        <div id="supplementation" class="supplementation" runat = "server"></div>
        <div id="Performance" class="radio-group clearfix">
            <asp:RadioButtonList ID="RBLPerform" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" Width="100%">
            </asp:RadioButtonList>
            <asp:RequiredFieldValidator ID="RqFV" runat="server" ErrorMessage="どれか１つを選択して頂けますでしょうか" ControlToValidate="RBLPerform" CssClass="RqValid" SetFocusOnError="True" Display="Dynamic">●どれか１つを選択して頂けますでしょうか</asp:RequiredFieldValidator>
        </div>
        
    </div>