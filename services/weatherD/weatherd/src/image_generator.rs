use std::fs::*;
use std::io::Read;
use magick_rust::{MagickWand, magick_wand_genesis};
use std::io::prelude::*;
use crate::push_message_handler::CreateSourceDto;

pub fn generate(colors : &Vec<String>, dto : &CreateSourceDto) -> String {
    let mut file = File::open("read_mme.svg").unwrap();

    let mut content = String::new();

    file.read_to_string(&mut content).unwrap();

    let s2 = content
        .replace("{{kekeke_0}}", &colors[0])
        .replace("{{kekeke_1}}", &colors[1])
        .replace("{{kekeke_2}}", &colors[2])
        .replace("{{kekeke_3}}", &colors[3])
        .replace("{{kekeke_4}}", &colors[4])
        .replace("{{kekeke_5}}", &colors[5])
        .replace("{{kekeke_6}}", &colors[6])
        .replace("{{kekeke_7}}", &colors[7])

        .replace("{{name}}", &dto.source)
        .replace("{{population}}", &dto.population)
        .replace("{{place_status}}", &dto.place_status)
        .replace("{{race}}", &dto.race)
        .replace("{{danger}}", &dto.danger);


    return s2;
}

pub fn generate_png(colors : &Vec<String>, dto : &CreateSourceDto ) -> Vec<u8>{
    let content = generate(&colors, dto);

    let mut file = File::create(&(dto.source.to_string() + ".svg")).unwrap();
    file.write_all(content.as_bytes());

    let wand = MagickWand::new();

    match wand.read_image(&(dto.source.to_string() + ".svg")) {
        Ok(r) => {},
        Err(e) => panic!(".")
    }

    match wand.write_image_blob("png") {
        Ok(r) => {
            let mut file2 = File::create(&(dto.source.to_string() + ".png")).unwrap();

            match file2.write_all(&r) {
                Ok(rr) => {
                    return r;
                },
                Err(e) => panic!("")
            }
        },
        Err(e) => panic!("")
    }
}