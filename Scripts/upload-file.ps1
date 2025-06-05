. "$PSScriptRoot/constants.ps1"

$FILE_PATH = "../TestFiles/ContractTest_1/Input/Contracts.json"

aws --endpoint-url $ENDPOINT s3 cp $FILE_PATH "s3://$INPUT_BUCKET_NAME/Contracts.json" --region $REGION
