<%@ Page Language="VB" Culture="ja-JP" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head runat="server">
    <title>メイリオフォントの使用方法について</title>
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <meta charset="utf-8" />
    <link href="CSS/stl_basic.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <h2>メイリオフォントを使うための設定</h2>
      <br />
    </div>
      <div>
      <ol>
        <li>インストールされていない場合は、こちらからダウンロードしてインストールしてください。<br />
          &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="http://www.microsoft.com/downloads/details.aspx?FamilyID=f7d758d2-46ff-4c55-92f2-69ae834ac928&amp;displaylang=ja&amp;Hash=9QmPTiwvbGtcGoQvIf2UX3ueDzsnIoo0qyWFQqrVEK1mApNcC%2fMyUiBP8qcbIvjT77dGwr2K2hyt1m0wXtGqDA%3d%3d" target="_blank">Windows XP 向け ClearType 対応日本語フォント（メイリオ）</a>
          <br />
        </li>
        <li>メイリオフォントを効果的に使用する方法</li>
      </ol>
      <ul>
        <li>デスクトップの適当な場所(何もない場所)でマウスを右クリックします。</li>
        <li>「プロパティ」をクリックします。</li>
        <li>「デザイン」タブをクリックし、右下の「効果」ボタンをクリックします</li>
        <li>「次の方法でスクリーンフォントの縁を滑らかにする」から「ClearType」を選択し、「OK」をクリックします。<br />
          <asp:Image ID="Image1" runat="server" ImageUrl="~/img/effect.png" /></li>
        <li>最後に「画面のプロパティ」の右下の「OK」をクリックして完了です。<br />
        </li>
      </ul>
    </div>
    </form>
</body>
</html>
