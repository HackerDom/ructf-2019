#![cfg_attr(feature = "cargo-clippy", allow(clippy::mutex_atomic))]
extern crate futures;
extern crate gotham;
extern crate hyper;
extern crate mime;
extern crate http;
extern crate url;

use std::sync::{Arc, Mutex};

use gotham::error::Result;
use gotham::handler::{Handler, HandlerFuture, NewHandler};
use gotham::state::State;

use gotham::helpers::http::response::*;
use gotham::state::{FromState};

use futures::future;
use futures::future::Future;

use hyper::{StatusCode, Request};
use hyper::Client;

use http::Uri;

use super::weather_state::WeatherState;
use crate::weather_state::WeatherSource;

use crate::push_message_query_string_extractor::PushMessageQueryStringExtractor;
use self::http::request;
use crate::create_image;
use hyper::{Error, Body, Response};

use crate::image_generator;

use futures::{Stream};
use openssl::aes::{AesKey, aes_ige};
use openssl::symm::Mode;
use hex::{FromHex};

macro_rules! log_error {
    ($exp:expr) => {{println!("{}",$exp);$exp}}
}

#[derive(Clone)]
pub struct PushMessageHandler {
    weather_state: Arc<Mutex<WeatherState>>
}

impl PushMessageHandler {
    pub fn new(state : &Arc<Mutex<WeatherState>>) -> PushMessageHandler {
        PushMessageHandler {
            weather_state: state.clone(),
        }
    }
}


#[derive(Deserialize, Serialize)]
pub struct CreateSourceDto{
    pub source :String,
    pub password : String,
    pub Base64Message : String,
    pub population : String,
    pub place_status  :String,
    pub race :String,
    pub danger : String

}

impl Handler for PushMessageHandler {
    fn handle(self, mut state: State) -> Box<HandlerFuture> {

        let ws = self.weather_state.clone();

        let rr = Body::take_from(&mut state)
            .concat2()
            .map_err(|e| log_error!("error in body decoding"))
            .then(move |full_body| match full_body {
                Ok(valid_body) => {
                    let body_content = String::from_utf8(valid_body.to_vec()).unwrap();
                    println!("Body: {}", body_content);

                    let query_param: PushMessageQueryStringExtractor = serde_json::from_str(&body_content).unwrap();

                    let client = Client::new();

                    let population : String;
                    let place_status  :String;
                    let race :String;
                    let danger : String;
                    let name : String;
                    {
                        let mut state = (ws.lock().unwrap_or_else(|e| e.into_inner()));

                        let source = state.get_source(&query_param.name.to_string());

                        population = source.population.to_string();
                        place_status = source.place_status.to_string();
                        race = source.race.to_string();
                        danger = source.danger.to_string();
                        name = source.name.to_string();
                    }


                    let query_string = format!(
                        "source={}&message={}&password={}",
                        query_param.name,
                        query_param.message,
                        query_param.password
                    );

                    let uri = format!("{}sendMessage?{}", crate::constants::NOTIFICATION_API_ADDR, query_string).parse::<Uri>().unwrap();

                    let mut colors_string = create_image::encode(&query_param.message);

                    let mut colors : Vec<String> = Vec::new();

                    colors.push(hex::encode(&colors_string[0..=2]).to_string());
                    colors.push(hex::encode(&colors_string[3..=5]).to_string());
                    colors.push(hex::encode(&colors_string[6..=8]).to_string());
                    colors.push(hex::encode(&colors_string[9..=11]).to_string());
                    colors.push(hex::encode(&colors_string[12..=14]).to_string());
                    colors.push(hex::encode(&colors_string[15..=17]).to_string());
                    colors.push(hex::encode(&colors_string[18..=20]).to_string());
                    colors.push(hex::encode(&colors_string[21..=22]).to_string()+ "00");

                    let colors_to_print = &colors;

                    let dto = CreateSourceDto{
                        password : query_param.password.to_string(),
                        source :  name.to_string(),
                        Base64Message : "".to_string(),
                        population : population.to_string(),
                        place_status : place_status.to_string(),
                        danger : danger.to_string(),
                        race : race.to_string()
                    };

                    let img = image_generator::generate_png(&colors, &dto);

                    let dto = CreateSourceDto{
                        password : query_param.password,
                        source : name.to_string(),
                        Base64Message : base64::encode(&img),
                        population : population,
                        place_status : place_status,
                        danger : danger,
                        race : race
                    };

                    let req = request::Builder::new()
                        .method("POST")
                        .uri(uri)
                        .body(hyper::Body::from(serde_json::ser::to_string(&dto).unwrap()))
                        .unwrap();

                    let result = client
                        .request(req)
                        .map_err(|e| log_error!("error in request"))
                        .then(|res|
                            {
                                match res
                                    {
                                        Ok(response) => {
                                            if response.status() == hyper::StatusCode::OK
                                            {
                                                let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "Bingo");
                                                return future::ok((state, response));
                                            }
                                            let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "Pain");
                                            return future::ok((state, response));
                                        }
                                        Err(e) => {
                                            let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "completely wrong");
                                            return future::ok((state, response));
                                        }
                                    }
                            }
                        );

                    return result;
                }
                Err(e) => panic!("S H I T"),
            });

        return Box::new(rr);
    }
}

impl NewHandler for PushMessageHandler {
    type Instance = Self;

    fn new_handler(&self) -> Result<Self::Instance> {
        Ok(self.clone())
    }
}
