<%@ Page Language="VB" Culture="ja-JP" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="ja">
<head id="Head1" runat="server">
    <title>Javascriptの設定方法について</title>
    <meta name="viewport" content="width=device-width, initial-scale=0.8, minimum-scale=0.6, maximum-scale=5.0, user-scalable=yes" />
    <meta charset="utf-8" />
    <link href="CSS/stl_basic.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
      <H2>
        Javascriptを有効にするための設定</H2>
        <p>&nbsp;</p>
      <p>・Internet Explorerの場合</p>
    <div>
      <ol>
        <li>メニューバーの「ツール」をクリックし、「インターネットオプション」を選択します。 </li>
        <li>「セキュリティ」のタブを選択し、<strong><span style="font-size: 18pt"></span></strong>下の「レベルのカスタマイズ<strong><span
          style="font-size: 18pt"></span></strong><strong><span style="font-size: 18pt"></span></strong>」ボタンをクリックします。
        </li>
        <li>[スクリプト]－[アクティブ スクリプト]－[有効にする]を選択します。 </li>
      </ol>
    </div>
      <p>&nbsp;</p>
      <p>・Firefoxの場合</p>
      <ol>
        <li>メニューバーの「ツール」をクリックし、「オプション」をクリックします。 </li>
        <li>「コンテンツ」タブを選択し、「Javascriptを有効にする」にチェックを入れます。</li>
      </ol>
      <p>&nbsp;</p>
      <p>・Google Chromeの場合</p>
      <ol>
        <li>右上のレンチマークをクリックし、「オプション」を選択します。</li>
        <li>「高度な設定」タブを選択し、「コンテンツの設定」をボタンをクリックします。</li>
        <li>「Javascript」で、「すべてのサイトで Javascript の実行を許可する（推奨）」を選択します。</li>
      </ol>
      <p>&nbsp;</p>
    </form>
</body>
</html>
