using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorCityBuildings : Generator {

	protected override void build() {
		throw new System.NotImplementedException();
	}
	public override GENERATOR_TYPE getType() {
		return GENERATOR_TYPE.CITY_BUILDINGS;
	}
}
