DRAFTERS - REQUISITION + INVENTORY + PURCHASING + DELIVERY   (ASP.NET Web Forms, C#, MySQL)
=========================================================================================
One application, one login page, four role-based dashboards.

STEP 1 - DATABASE (MySQL Workbench)
  New database:      run Database\requisition_db_mysql.sql  (it already contains EMP-0004 / role Delivery).
  Existing database: run Database\add_delivery_user.sql once (keeps your data, allows the role "Delivery",
                     adds EMP-0004).

STEP 2 - OPEN THE PROJECT
  1. Double-click Drafters.sln  (or Drafters.slnx).
  2. Package Manager Console (Default project = Drafters):
         Install-Package MySql.Data.EntityFramework -Version 8.0.33
  3. Build > Rebuild Solution, then F5.

LOGIN  (password for every account: Pass@123).  The role is read from the Users table - nobody picks a role.
  EMP-0001  Admin       -> Admin Dashboard        (Default.aspx)
  EMP-0002  Inventory   -> Inventory Dashboard    (InventoryDashboard.aspx)
  EMP-0003  Purchasing  -> Purchasing Dashboard   (PurchasingDashboard.aspx)
  EMP-0004  Delivery    -> Delivery Dashboard     (Delivery\Dashboard.aspx)
  EMP-1001 / EMP-1002  Employee (Requester) -> Requisition Form

WHO CAN OPEN WHAT  (enforced on the server in every page; a wrong URL redirects to the user's own dashboard)
  Admin       Default (Admin Dashboard), Requests / RequestDetails / SlipPrint (Requisition Management),
              Inventory / ItemEdit (Inventory Management), Replenishment (Purchasing Management),
              DeliveryManagement, Users (User Management), Notifications.   NOT the Delivery area.
  Inventory   InventoryDashboard, Requests / RequestDetails / SlipPrint, Inventory / ItemEdit, Replenishment, Notifications.
  Purchasing  PurchasingDashboard, Replenishment, Notifications.
  Delivery    Delivery\*  (Dashboard, Delivery Notices, Receiving, Discrepancies, Verification, Invoice & Release,
              Delivery Records), Notifications.
  Employee    RequestForm, Requests / RequestDetails / SlipPrint (own requests only), Notifications.

ADMIN AND DELIVERY
  The Admin sees "Delivery Management" (DeliveryManagement.aspx): totals, search/filter, full record view and the
  discrepancy list - read-only oversight. It never opens the Delivery employee dashboard.
  The delivery work (notices, receiving, discrepancy follow-up, verification, invoice and release) is done only by
  the Delivery role in the Delivery area.

USER MANAGEMENT (Admin)  Users.aspx: search, add user, edit name/username/department/role, reset password to Pass@123,
  activate/deactivate. An admin cannot change their own role or deactivate themselves. Role changes and
  deactivation take effect on the user's next page request.

DELIVERY DATA - NOT CONNECTED TO MYSQL YET (by request)
  Delivery data is in memory (resets when the app restarts). Every Delivery page and Delivery Management use only
  Drafters\Delivery\Services\IDeliveryRepository.cs. For the MySQL phase, write a MySqlDeliveryRepository that
  implements it and change the one line in DeliveryData. The rest of the application needs no change.

FOLDER MAP (Delivery)
  Delivery\*.aspx / .cs / .designer.cs   Delivery-role pages
  Delivery\DeliveryBasePage.cs           Delivery-only access + permissions (extends the Drafters BasePage)
  Delivery\Models, Delivery\Services     entities, IDeliveryRepository, in-memory repository, HTML helpers
  Content\delivery.css                   the few Delivery styles not covered by the Drafters theme
