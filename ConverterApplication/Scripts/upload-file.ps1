. "$PSScriptRoot/constants.ps1"

$FILE_NAME = "Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json"

aws --endpoint-url $ENDPOINT --region $REGION s3 cp "./"$FILE_NAME" "s3://$INPUT_BUCKET_NAME/$FILE_NAME" 
