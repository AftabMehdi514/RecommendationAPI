using System.Data.SqlTypes;
using System.Reflection.Metadata;
using Microsoft.Data.SqlClient;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/rcmnd/{id}", (int id) =>
{
    

    List<object> products= new();

    string conStr="Data Source=FurnitureDB.mssql.somee.com;Initial Catalog=FurnitureDB;User id=aftabmehdi514_SQLLogin_1;Password=2czuxl4oz4;TrustServerCertificate=True;";

    using SqlConnection conn=new SqlConnection(conStr);

      conn.Open();



        //for now lets show all products later we will do recommended only...

      string query="select * from product_t";

      // lets make it acutal recommendation systemm
      // lets implement 
      // What we gonna do is Lets grab all orders wehre this product is present
      // then from those orders we will  show other products that are infact
      // being ordered together in same order 
      // lets try to build query
      // Getting Orders where that product exist: select Order_Id from order_line_t where product_id=1 
      // now all other products that are in this order but not this product
      string rcmndQuery="select product_id from order_line_t where order_id in(select Order_Id from order_line_t where product_id=1) AND Product_Id <> 1 ";
        // this will give some ids again and again instead of taking distinck lets get advantage of this 
        // and try to show in order by frequencye.... using Verbatim String for multiline
        string rcmndFinalQuery=@"
        
                select product_id, count(*) as prdscnts
                from order_line_t
                where order_id in (
                
                select order_id from order_line_t 
                where  product_id=@id 

                )
                 and product_id<>@id
                 group by product_id
                 order by prdscnts desc

        ";






     SqlCommand cmd= new SqlCommand(rcmndFinalQuery,conn);
     cmd.Parameters.AddWithValue("@id",id);
    SqlDataReader dr= cmd.ExecuteReader();

    while (dr.Read())
    {
        products.Add(new
        {
            productId=dr["Product_Id"]
           
        });
    
    }
    return products;
});

// now lets make another end point , the previous end point is for 
//recommendations on order page 
// now lets make end point to recomemnd products on custoemr dashboard
// this will receive customer id ...






app.MapGet("/dashboardRcmnd/{cid}", (int cid) =>
{
    List <int> prods=new();

    // now lets get products from db

string conStr="Data Source=FurnitureDB.mssql.somee.com;Initial Catalog=FurnitureDB;User id=aftabmehdi514_SQLLogin_1;Password=2czuxl4oz4;TrustServerCertificate=True;";

    using SqlConnection conn=new SqlConnection(conStr);
     conn.Open();
  // lets develop query step by step for filtereing....
   // lets get custoemr state first
      string q1="select customer_State from customer_t where customer_id=3005";// 
    // 2nd step lets get all other custoemr of that state
    string q2=@"select customer_id
    from customer_t
    where Customer_state='kr'
    And customer_id <> 1
    ";

 // ok, so now finds products that thse customer bought
 // using q2 as inneer querye

  // need to take join

  string q3=@"
        select ol.Product_id
        from order_t  o
        join order_line_t  ol ON o.order_id=ol.order_id
        where o.customer_id in (q2)

  ";

  // so final quere  wiht parametrized customer id 
  string q4=@"

    select ol.Product_id,count(*) as prods
    from order_t o
     JOIN Order_Line_t ol
   ON o.Order_Id = ol.Order_Id

     where o.customer_id in (

                    select customer_id 

                    from customer_t

                    where customer_state in(
                                    select customer_state from customer_t where customer_id=@cid
                                            )
                    and customer_id<>@cid
     )
     group by ol.Product_id
        order by  prods DESC
  
  
  ";







    // string query2="select * from product_t";
     SqlCommand cmd2  = new SqlCommand(q4,conn);
     cmd2.Parameters.AddWithValue("@cid",cid);
     SqlDataReader dr=cmd2.ExecuteReader();
     while (dr.Read())
    {
        prods.Add(Convert.ToInt32(dr["Product_id"]));
    }


    return prods;

});




















app.MapGet("/", () => "Hello World! This is Aftab ");
app.MapGet("/Employees", () => new[]
{
    new Employee("emp1"),new Employee("emp2")


});



//app.MapGet("/users/{id}?", (int id,User user) => $"This what valsue i will give back {id}");

app.MapPost("/users/{id}",
(int id,User user) => $"receive this user naem {user.Name} wiht id {user.Id}"
);


app.Run();

record Employee(string Name);
record User(int Id, string Name);
