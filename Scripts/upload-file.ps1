. "$PSScriptRoot/constants.ps1"

$FILE_PATH = "../TestFiles/ContractTest_1/Input/Contract.xml"

aws --endpoint-url $ENDPOINT s3 cp $FILE_PATH "s3://$BUCKET_NAME/Contract.xml" --region $REGION
