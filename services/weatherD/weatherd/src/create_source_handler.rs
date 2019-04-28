extern crate futures;
extern crate gotham;
extern crate hyper;
extern crate mime;

use std::sync::{Arc, Mutex};

use gotham::error::Result;
use gotham::handler::{Handler, HandlerFuture, NewHandler};
use gotham::state::State;

use gotham::helpers::http::response::*;

use hyper::{StatusCode, Error, Body, Response};
use hyper::Client;

use http::{Uri, request};

use futures::future;
use futures::future::Future;

use gotham::state::{FromState};

use crate::create_source_query_string_extractor::CreateSourceQueryStringExtractor;
use crate::push_message_query_string_extractor::PushMessageQueryStringExtractor;

use crate::weather_state::WeatherSource;
use crate::weather_state::WeatherState;
use chrono::{DateTime, Utc};


use tokio_core::reactor::Core;
use futures::{Stream};

use gotham::handler::{IntoHandlerError};

use hyper::{Method};
use futures::future::Either;
use regex::Regex;
use crate::create_source_dto::CreateSourceDto;

macro_rules! log_error {
    ($exp:expr) => {{println!("{}",$exp);$exp}}
}


#[derive(Clone)]
pub struct CreateSourceHandler {
    weather_state: Arc<Mutex<WeatherState>>,
}

impl CreateSourceHandler {
    pub fn new(state : &Arc<Mutex<WeatherState>>) -> CreateSourceHandler {
        CreateSourceHandler {
            weather_state: state.clone()
        }
    }
}


impl Handler for CreateSourceHandler {
    fn handle(self, mut state: State) -> Box<HandlerFuture> {
        println!("create started");
        let rr = Body::take_from(&mut state)
            .concat2()
            .then(move |full_body| match full_body {
                Ok(valid_body) => {
                    let body_content = String::from_utf8(valid_body.to_vec()).unwrap();
                    println!("Body: {}", body_content);

                    let create_source_dto: CreateSourceQueryStringExtractor = serde_json::from_str(&body_content).unwrap();

                    let time = Utc::now();
                    let time_string = time.timestamp();
                    let is_public = create_source_dto.is_public == true;

                    let query_string = format!(
                        "source={}&time={}&password={}",
                        create_source_dto.name,
                        time_string,
                        create_source_dto.password
                    );

                    let danger_class = match create_source_dto.race.as_ref() {
                        "supermutants" => "peaceful",
                        "humans" => "peaceful",
                        "aliens" => "danger",
                        "reptiloids" => "keter",
                        _ => "unknown"
                    }.to_string();

                    let place_status : String = self.choose_place_status(&create_source_dto).to_string();

                    let uri = format!("{}addUserInfo?{}", crate::constants::NOTIFICATION_API_ADDR, query_string).parse::<Uri>().unwrap();

                    let client = Client::new();

                    let dto = CreateSourceDto{
                        source : create_source_dto.name.to_string(),
                        password : create_source_dto.password.to_string(),
                        timestamp : time.timestamp()
                    };

                    println!("shh");


                    let name = xml::escape::escape_str_attribute(&create_source_dto.name);
                    let population  = xml::escape::escape_str_attribute(&create_source_dto.population);

                    let source = WeatherSource
                        {
                            password: create_source_dto.password.to_string(),
                            name: name.to_string(),
                            token: body_content.to_string(),
                            place_status: population.to_string(),
                            danger : danger_class.to_string(),
                            race : create_source_dto.race.to_string(),
                            population : population.to_string()
                        };

                    {
                        let mut state = (self.weather_state.lock().unwrap_or_else(|e| e.into_inner()));

                        if state.contains_source(&create_source_dto.name) {
                            let source = state.get_source(&create_source_dto.name);
                            if &create_source_dto.password != &source.password {
                                panic!("password mismatch");
                            }
                        } else {
                            state.add_source(&create_source_dto.name, source);
                        }
                    }

                    println!("uri {}", uri);

                    let req = request::Builder::new()
                        .method("POST")
                        .uri(uri)
                        .body(Body::from(serde_json::ser::to_string(&dto).unwrap()))
                        .unwrap();

                    let result = client
                        .request(req)
                        .map_err(|e| log_error!("error in request"))
                        .then( |res|
                            {
                                match res
                                    {
                                        Ok(response) => {
                                            println!("request received");
                                            response
                                                .into_body()
                                                .concat2()
                                                .then( move|full_body| {
                                                    println!("start reading body");

                                                    match full_body {
                                                        Ok(valid_body) => {
                                                            let body_content = String::from_utf8(valid_body.to_vec()).unwrap();
                                                            let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, Body::from(body_content.to_string()));
                                                            return future::ok((state, response));
                                                        },
                                                        Err(e) => {
                                                            panic!("Body id not valid, {}", e)
                                                        }
                                                    }
                                                })
                                        },

                                        Err(e) => {
                                            panic!("something is wrong {}", e);
                                        }
                                    }
                            });
                    println!("sended something create source handled");

                    return result;
                }
                Err(e) => {
                    log_error!(e);
                    panic!("S")
                }
            });

        return Box::new(rr);
    }
}

impl NewHandler for CreateSourceHandler {
    type Instance = Self;

    fn new_handler(&self) -> Result<Self::Instance> {
        Ok(self.clone())
    }
}


impl CreateSourceHandler {
    pub fn choose_place_status(&self, dto : &CreateSourceQueryStringExtractor) -> String{

        let re = Regex::new(r"^\d{1,10}$").unwrap();

        if !re.is_match(&dto.population){
            return "invalid population".to_string();
        }

        let place_status : String;
        let landscape = dto.landscape.to_string();

        match dto.population.parse::<i128>() {
            Ok(r) =>

                match r {
//                    r if r == 0 => ("unpopulated ".to_string() + &landscape),
                    r if r == 0 => "unpopulated landscape".to_string(),
                    r if r < 100 => "hamlet".to_string(),
                    r if r < 1000 => "village".to_string(),
                    r if r < 10000 => "town".to_string(),
                    _ => "city".to_string(),

                }
            Err(e) =>{
                  log_error!(format!("Cannot parse population for populated place {} : {}", dto.name , e))
            }
        }
    }
}
