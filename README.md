# APBD_6
Warehouse database. 

We've modified the AddProductToWarehouse method to execute the stored procedure named AddProductToWarehouse.
We've set up the parameters required by the stored procedure based on the properties of the ProductWarehouseRequest object.
We execute the stored procedure using command.ExecuteScalar() to get the result.
In case of any error during the execution of the stored procedure, we handle it and return an appropriate error response with the error message.

for implementing AddProductToWarehouse:
Check if the product with the given ID exists.
Check if the warehouse with the given ID exists.
Ensure that the amount value passed in the request is greater than 0.
Verify if there is a corresponding product purchase order in the Order table.
Check if the order has been completed.
Update the FulfilledAt column of the order with the current date and time.
Insert a record into the Product_Warehouse table.
Calculate the Price column value based on the product price multiplied by the amount.
Return the value of the primary key generated for the inserted record.
