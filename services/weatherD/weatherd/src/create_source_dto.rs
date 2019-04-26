#[derive(Deserialize, Serialize)]
pub struct CreateSourceDto{
    pub source :String,
    pub password : String,
    pub timestamp : i64
}
