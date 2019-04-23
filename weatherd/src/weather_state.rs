use std::string;
use uuid::Uuid;

//extern crate chrono;

use chrono::prelude::*;
use std::collections::HashMap;

pub enum Race {
    Human,
    Supermutant,
}



pub struct WeatherSource {
    pub name : String,
    pub password : String,
    pub token : String,
//    discovery_date : DateTime<Utc>,
//    planet_name : String,
//    population : u64,
//    population_race : Race,
//    internal_id : Uuid,
}



//impl WeatherSource {
//    pub fn new(name : String, planet_name : String, population : u64, ) -> WeatherSource {
//        return WeatherSource {
//            name: name,
////            planet_name : planet_name,
////            population : population,
////            discovery_date : Utc::now(),
////            internal_id : Uuid::
//        };
//    }
//}

impl WeatherSource {
    fn get_name(self) -> String{
        return self.name;
    }
}



impl WeatherState {
    pub fn add_source(&mut self, name : &str, weather_source : WeatherSource){
        self.sources.insert(name.to_string(), weather_source);
    }

    pub fn get_all_sources(&mut self) -> &HashMap<String, WeatherSource>{
        return &self.sources;
    }

    pub fn get_source(&mut self, name : &str) -> &WeatherSource{
        return self.sources.get(name).unwrap();
    }
}


pub struct WeatherState {
    sources : HashMap<String, WeatherSource>
}

impl WeatherState {
    pub fn new() -> WeatherState {
       return WeatherState {
//            started_at: SystemTime::now(),
////            visits: Arc::new(Mutex::new(0)),
            sources: HashMap::new()
        }
    }
}
