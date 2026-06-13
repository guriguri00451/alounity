#!/bin/bash
# ローカルHTTPS証明書生成スクリプト
# mkcertを使用してlocalhostとローカルIPの証明書を生成します

set -e

# 色の定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== ローカルHTTPS証明書生成スクリプト ===${NC}"

# mkcertがインストールされているか確認
if ! command -v mkcert &> /dev/null; then
    echo -e "${RED}エラー: mkcertがインストールされていません${NC}"
    echo ""
    echo "以下のコマンドでmkcertをインストールしてください："
    echo "  macOS: brew install mkcert && mkcert -install"
    echo "  Linux: sudo apt install mkcert || sudo yum install mkcert"
    echo "  Windows: choco install mkcert && mkcert -install"
    echo ""
    echo "詳細: https://github.com/FiloSottile/mkcert"
    exit 1
fi

echo -e "${GREEN}✓ mkcertが見つかりました${NC}"

# ローカルCAをインストール
mkcert -install
echo -e "${GREEN}✓ ローカルCAがインストールされました${NC}"

# 証明書ディレクトリを作成
CERT_DIR="certs"
mkdir -p "$CERT_DIR"

echo -e "${GREEN}✓ 証明書ディレクトリを作成しました: $CERT_DIR${NC}"

# ローカルIPアドレスを取得
LOCAL_IP=$(ifconfig | grep "inet " | grep -v 127.0.0.1 | head -1 | awk '{print $2}')

# 既存の証明書を削除
rm -f "$CERT_DIR"/*.pem

# 証明書を生成（すべてのドメインを1つの証明書に）
echo "証明書を生成中..."

if [ -n "$LOCAL_IP" ]; then
    echo -e "${GREEN}ローカルIPアドレスを検出: $LOCAL_IP${NC}"
    cd "$CERT_DIR"
    mkcert -cert-file server.pem -key-file server-key.pem localhost 127.0.0.1 ::1 "$LOCAL_IP"
    cd ..
else
    echo -e "${YELLOW}警告: ローカルIPアドレスを検出できませんでした${NC}"
    cd "$CERT_DIR"
    mkcert -cert-file server.pem -key-file server-key.pem localhost 127.0.0.1 ::1
    cd ..
fi

echo ""
echo -e "${GREEN}=== 証明書生成完了 ===${NC}"
echo ""
echo "生成された証明書:"
ls -la "$CERT_DIR"/*.pem 2>/dev/null || echo "証明書が見つかりません"
echo ""
echo -e "${GREEN}次のステップ:${NC}"
echo "1. HTTPSサーバーを起動: npm run dev:https"
echo "2. ブラウザで https://localhost:3000 にアクセス"
echo "3. スマホからアクセス: https://<PCのIPアドレス>:3000"
echo ""
echo -e "${YELLOW}注意:${NC}"
echo "- スマホからアクセスする場合は、同じWi-Fiネットワークに接続してください"
echo "- iOSの場合、設定 > 一般 > 情報 > 証明書信頼設定 で証明書を信頼してください"
