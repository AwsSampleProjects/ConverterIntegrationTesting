. "$PSScriptRoot/constants.ps1"

$FILE_PATH = "../TestFiles/ContractTest_1/Input/Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json"

aws --endpoint-url $ENDPOINT s3 cp $FILE_PATH "s3://$INPUT_BUCKET_NAME/Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json" --region $REGION
