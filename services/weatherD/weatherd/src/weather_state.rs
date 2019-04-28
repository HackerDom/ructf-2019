use std::string;
use uuid::Uuid;

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

    pub population : String,
    pub place_status  :String,
    pub race :String,
    pub danger : String
}



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

    pub fn contains_source(&mut self, name : &str) -> bool {
        return self.sources.contains_key(name);
    }
}


pub struct WeatherState {
    sources : HashMap<String, WeatherSource>
}

impl WeatherState {
    pub fn new() -> WeatherState {
       return WeatherState {
            sources: HashMap::new()
        }
    }
}
