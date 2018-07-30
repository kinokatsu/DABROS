<%@ Control Language="VB" AutoEventWireup="false" CodeFile="wds.ascx.vb" Inherits="UserContorl_wds" %>

    <div runat=server id="wind" class="txt">
        <div runat=server id="nor" class="tp mdl">北</div>
        <div runat=server id="wes" class="lft mdl">西</div>
        <div runat=server id='wds' class='wds cnt mdl'>
            <asp:Literal ID="Lit2" runat="server"></asp:Literal>
        </div>
        <div runat=server id="eas" class="rgt mdl">東</div>
        <div runat=server id="sou" class="tp mdl">南</div>    
    </div>
